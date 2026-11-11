import base64
import json
import queue
import sys
import threading
from urllib.parse import quote, urlsplit

import gi

gi.require_version("Gst", "1.0")
gi.require_version("GstApp", "1.0")
gi.require_version("GstSdp", "1.0")
gi.require_version("GstWebRTC", "1.0")
from gi.repository import GLib, Gst, GstSdp, GstWebRTC

Gst.init(None)


class Peer:
    def __init__(self):
        self.loop = GLib.MainLoop()
        self.pipeline = None
        self.rtc = None
        self.outgoing = queue.Queue(64)
        self.frames = {}
        self.frame_lock = threading.Lock()
        self.local_sent = False
        self.remote_set = False
        self.pending_local = []
        self.pending_remote = []
        self.mids = {}
        self.closed = False
        self.audio_packets = 0
        self.video_frames = 0
        self.writer = threading.Thread(target=self.write, daemon=True)
        self.writer.start()

    def emit(self, message):
        try:
            self.outgoing.put_nowait(message)
        except queue.Full:
            raise RuntimeError("WebRTC event queue overflow")

    def write(self):
        while True:
            message = self.outgoing.get()
            print(json.dumps(message, separators=(",", ":")), flush=True)
            if message["type"] == "closed":
                return

    def fail(self, message):
        self.emit({"type": "error", "message": str(message)})
        if self.pipeline:
            self.pipeline.set_state(Gst.State.NULL)

    def command(self, message):
        try:
            command = message["command"]
            if command == "start":
                self.start(message["configuration"])
            elif command == "signal":
                self.signal(message["signal"])
            elif command == "close":
                self.close()
            else:
                raise ValueError("Unknown WebRTC command")
        except Exception as error:
            self.fail(error)
        return GLib.SOURCE_REMOVE

    def start(self, configuration):
        if self.pipeline is not None:
            raise RuntimeError("WebRTC already started")
        for plugin in ("webrtcbin", "nicesrc", "vp8enc", "opusenc", "jpegenc"):
            if not Gst.ElementFactory.find(plugin):
                raise RuntimeError("Missing GStreamer plugin: " + plugin)
        synthetic = configuration["camera"] == "test"
        camera = "videotestsrc is-live=true" if synthetic else "v4l2src name=camera"
        microphone = "audiotestsrc is-live=true" if synthetic else "pulsesrc name=microphone"
        self.pipeline = Gst.parse_launch(
            "webrtcbin name=rtc bundle-policy=max-bundle latency=100 "
            + camera + " ! videoconvert ! videoscale ! videorate ! "
            "video/x-raw,width=640,height=360,framerate=15/1 ! tee name=local "
            "local. ! queue leaky=downstream max-size-buffers=2 ! "
            "vp8enc deadline=1 target-bitrate=900000 keyframe-max-dist=30 ! "
            "rtpvp8pay pt=96 ! application/x-rtp,media=video,encoding-name=VP8,payload=96 ! rtc. "
            "local. ! queue leaky=downstream max-size-buffers=1 ! videorate ! "
            "video/x-raw,framerate=10/1 ! jpegenc ! appsink name=local_frames "
            "emit-signals=true sync=false max-buffers=1 drop=true "
            + microphone + " ! audioconvert ! audioresample ! audio/x-raw,rate=48000,channels=1 ! "
            "opusenc ! rtpopuspay pt=111 ! application/x-rtp,media=audio,encoding-name=OPUS,payload=111 ! rtc."
        )
        if not synthetic:
            self.pipeline.get_by_name("camera").set_property("device", configuration["camera"])
            if configuration["microphone"] != "default":
                self.pipeline.get_by_name("microphone").set_property("device", configuration["microphone"])
        self.synthetic = synthetic
        self.relay_only = configuration["relayOnly"]
        self.rtc = self.pipeline.get_by_name("rtc")
        self.ice = self.rtc.get_property("ice-agent")
        self.nice = self.ice.get_property("agent")
        self.nice.set_property("upnp", False)
        if configuration["relayOnly"]:
            self.rtc.set_property("ice-transport-policy", GstWebRTC.WebRTCICETransportPolicy.RELAY)
            self.nice.set_property("force-relay", True)
        for server in configuration["iceServers"]:
            for url in server["urls"]:
                if url.startswith(("turn:", "turns:")):
                    parsed = urlsplit(url.replace(":", "://", 1))
                    credentials = quote(server["username"], safe="") + ":" + quote(server["credential"], safe="")
                    turn = parsed.scheme + "://" + credentials + "@" + parsed.netloc
                    if parsed.query:
                        turn += "?" + parsed.query
                    if not self.rtc.emit("add-turn-server", turn):
                        raise RuntimeError("Invalid TURN server configuration")
                elif not configuration["relayOnly"] and url.startswith("stun:"):
                    self.rtc.set_property("stun-server", url.replace(":", "://", 1))
                elif not configuration["relayOnly"] and url.startswith("stuns:"):
                    raise RuntimeError("GStreamer does not support STUN/TLS; configure STUN or TURN/TLS")
        self.pipeline.get_by_name("local_frames").connect("new-sample", self.frame, "local")
        self.rtc.connect("on-ice-candidate", self.candidate)
        self.rtc.connect("pad-added", self.incoming)
        bus = self.pipeline.get_bus()
        bus.add_signal_watch()
        bus.connect("message::error", self.bus_error)
        if configuration["caller"]:
            self.rtc.connect("on-negotiation-needed", self.offer)
        if self.pipeline.set_state(Gst.State.PLAYING) == Gst.StateChangeReturn.FAILURE:
            raise RuntimeError("Cannot start GStreamer camera/microphone pipeline")
        GLib.timeout_add(100, self.flush_frames)
        GLib.timeout_add(1000, self.stats)

    def bus_error(self, bus, message):
        error, _ = message.parse_error()
        self.fail(error.message)

    def offer(self, rtc):
        GLib.idle_add(self.create_description, "offer")

    def promise(self, callback):
        def changed(promise, *_):
            try:
                reply = promise.get_reply()
                if reply and reply.has_field("error"):
                    raise RuntimeError(reply.get_value("error").message)
                # The reply is owned by the promise until this callback returns.
                GLib.idle_add(callback, reply.copy() if reply is not None else None)
            except Exception as error:
                GLib.idle_add(self.fail, str(error))
        return Gst.Promise.new_with_change_func(changed, None, None)

    def create_description(self, kind):
        if self.closed:
            return GLib.SOURCE_REMOVE
        def created(reply):
            try:
                description = reply.get_value(kind)
                sdp_message = description.sdp
                sdp = sdp_message.as_text()
                mids = {}
                index = -1
                for line in sdp.splitlines():
                    if line.startswith("m="):
                        index += 1
                    elif line.startswith("a=mid:"):
                        mids[index] = line[6:]
                def applied(_):
                    try:
                        self.mids = mids
                        self.emit({"type": "signal", "signal": {"kind": kind, "sdp": sdp}})
                        self.local_sent = True
                        for index, candidate in self.pending_local:
                            self.send_candidate(index, candidate)
                        self.pending_local.clear()
                    except Exception as error:
                        self.fail(error)
                    return GLib.SOURCE_REMOVE
                self.rtc.emit("set-local-description", description, self.promise(applied))
            except Exception as error:
                self.fail(error)
            return GLib.SOURCE_REMOVE
        self.rtc.emit("create-" + kind, None, self.promise(created))
        return GLib.SOURCE_REMOVE

    def candidate(self, rtc, index, candidate):
        GLib.idle_add(self.local_candidate, index, candidate)

    def local_candidate(self, index, candidate):
        if self.closed or not candidate:
            return GLib.SOURCE_REMOVE
        if self.local_sent:
            self.send_candidate(index, candidate)
        elif len(self.pending_local) < 256:
            self.pending_local.append((index, candidate))
        else:
            self.fail("Too many local ICE candidates")
        return GLib.SOURCE_REMOVE

    def send_candidate(self, index, candidate):
        if self.relay_only:
            parts = candidate.split()
            if len(parts) < 8 or parts[6:8] != ["typ", "relay"]:
                self.fail("Relay-only ICE produced a non-relay candidate")
                return
            # RFC 8828: libnice exposes the base address in relay metadata.
            # Hide only that metadata; never rewrite SDP or the relay endpoint.
            for position in range(8, len(parts) - 1, 2):
                if parts[position] == "raddr":
                    parts[position + 1] = "::" if ":" in parts[4] else "0.0.0.0"
                elif parts[position] == "rport":
                    parts[position + 1] = "0"
            candidate = " ".join(parts)
        self.emit({"type": "signal", "signal": {
            "kind": "candidate", "candidate": candidate,
            "sdpMid": self.mids[index], "sdpMLineIndex": index}})

    def signal(self, signal):
        if signal["kind"] == "candidate":
            if self.remote_set:
                self.rtc.emit("add-ice-candidate", signal["sdpMLineIndex"], signal["candidate"])
            elif len(self.pending_remote) < 256:
                self.pending_remote.append(signal)
            else:
                raise RuntimeError("Too many pending remote ICE candidates")
        else:
            result, sdp = GstSdp.SDPMessage.new_from_text(signal["sdp"])
            if result != GstSdp.SDPResult.OK:
                raise ValueError("Invalid remote SDP")
            kind = GstWebRTC.WebRTCSDPType.OFFER if signal["kind"] == "offer" else GstWebRTC.WebRTCSDPType.ANSWER
            description = GstWebRTC.WebRTCSessionDescription.new(kind, sdp)
            def applied(_):
                self.remote_set = True
                for candidate in self.pending_remote:
                    self.rtc.emit("add-ice-candidate", candidate["sdpMLineIndex"], candidate["candidate"])
                self.pending_remote.clear()
                if signal["kind"] == "offer":
                    self.create_description("answer")
                return GLib.SOURCE_REMOVE
            self.rtc.emit("set-remote-description", description, self.promise(applied))

    def incoming(self, rtc, pad):
        if pad.get_direction() != Gst.PadDirection.SRC:
            return
        decoder = Gst.ElementFactory.make("decodebin")
        decoder.connect("pad-added", self.decoded)
        self.pipeline.add(decoder)
        decoder.sync_state_with_parent()
        if pad.link(decoder.get_static_pad("sink")) != Gst.PadLinkReturn.OK:
            GLib.idle_add(self.fail, "Cannot attach WebRTC decoder")

    def decoded(self, decoder, pad):
        media = pad.get_current_caps().get_structure(0).get_name()
        if media.startswith("video/"):
            sink = Gst.parse_bin_from_description(
                "queue leaky=downstream max-size-buffers=2 ! videoconvert ! videoscale ! videorate ! "
                "video/x-raw,width=640,height=360,framerate=10/1 ! jpegenc ! "
                "appsink name=remote_frames emit-signals=true sync=false max-buffers=1 drop=true", True)
            sink.get_by_name("remote_frames").connect("new-sample", self.frame, "remote")
        elif media.startswith("audio/"):
            sink = Gst.parse_bin_from_description(
                "queue ! audioconvert ! audioresample ! "
                + ("fakesink" if self.synthetic else "autoaudiosink") + " name=remote_audio sync=false", True)
            sink.get_by_name("remote_audio").get_static_pad("sink").add_probe(
                Gst.PadProbeType.BUFFER, self.audio_buffer)
        else:
            GLib.idle_add(self.fail, "Unsupported remote media")
            return
        self.pipeline.add(sink)
        sink.sync_state_with_parent()
        if pad.link(sink.get_static_pad("sink")) != Gst.PadLinkReturn.OK:
            GLib.idle_add(self.fail, "Cannot attach decoded media")

    def audio_buffer(self, pad, probe):
        self.audio_packets += 1
        return Gst.PadProbeReturn.OK

    def frame(self, sink, target):
        sample = sink.emit("pull-sample")
        if sample is None:
            return Gst.FlowReturn.EOS
        buffer = sample.get_buffer()
        image = buffer.extract_dup(0, buffer.get_size())
        if len(image) > 1024 * 1024:
            GLib.idle_add(self.fail, "Decoded image exceeds frame limit")
            return Gst.FlowReturn.ERROR
        with self.frame_lock:
            self.frames[target] = base64.b64encode(image).decode("ascii")
        if target == "remote":
            self.video_frames += 1
        return Gst.FlowReturn.OK

    def flush_frames(self):
        if self.closed:
            return GLib.SOURCE_REMOVE
        with self.frame_lock:
            frames, self.frames = self.frames, {}
        for target, data in frames.items():
            # Preview/render frames are disposable, unlike signaling.
            if self.outgoing.qsize() < 4:
                self.emit({"type": "frame", "target": target, "data": data})
        return GLib.SOURCE_CONTINUE

    def stats(self):
        if self.closed:
            return GLib.SOURCE_REMOVE
        def received(reply):
            if self.closed:
                return GLib.SOURCE_REMOVE
            try:
                all_stats = {reply.nth_field_name(i): reply.get_value(reply.nth_field_name(i))
                             for i in range(reply.n_fields())}
                selected_id = None
                for item in all_stats.values():
                    if isinstance(item, Gst.Structure) and item.has_field("selected-candidate-pair-id"):
                        selected_id = item.get_value("selected-candidate-pair-id")
                pair = all_stats.get(selected_id)
                local = all_stats.get(pair.get_value("local-candidate-id")) if pair else None
                local_type = local.get_value("candidate-type") if local else None
                state = self.rtc.get_property("connection-state").value_nick
                if state == "connected" and local_type is None:
                    state = "checking"
                self.emit({"type": "status", "status": {
                    "connectionState": state, "localCandidateType": local_type,
                    "audioPackets": self.audio_packets, "videoFrames": self.video_frames}})
            except Exception as error:
                self.fail(error)
            return GLib.SOURCE_REMOVE
        self.rtc.emit("get-stats", None, self.promise(received))
        return GLib.SOURCE_CONTINUE

    def close(self):
        self.closed = True
        if self.pipeline:
            self.pipeline.set_state(Gst.State.NULL)
        self.emit({"type": "closed"})
        self.loop.quit()


def main():
    peer = Peer()

    def read():
        try:
            for line in sys.stdin:
                if len(line) > 65536:
                    raise ValueError("WebRTC command exceeds size limit")
                GLib.idle_add(peer.command, json.loads(line))
        except Exception as error:
            GLib.idle_add(peer.fail, str(error))
        GLib.idle_add(peer.close)

    threading.Thread(target=read, daemon=True).start()
    peer.loop.run()
    peer.writer.join(timeout=3)


if __name__ == "__main__":
    main()
