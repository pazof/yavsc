package fr.pschneider.postit;

import android.content.Context;
import org.json.JSONArray;
import org.json.JSONObject;
import org.webrtc.*;
import org.webrtc.audio.JavaAudioDeviceModule;
import java.util.*;
import java.util.concurrent.*;

/** Keeps all SDK operations and asynchronous SDP callbacks on one executor. */
public final class WebRtcPeer {
    private final Context context;
    private final EglBase egl = EglBase.create();
    private final SurfaceViewRenderer local;
    private final SurfaceViewRenderer remote;
    private final ExecutorService executor = Executors.newSingleThreadExecutor();
    private final ArrayBlockingQueue<String> events = new ArrayBlockingQueue<>(64);
    private final List<IceCandidate> pendingLocal = new ArrayList<>();
    private final List<IceCandidate> pendingRemote = new ArrayList<>();
    private PeerConnectionFactory factory;
    private PeerConnection peer;
    private JavaAudioDeviceModule audioModule;
    private VideoCapturer capturer;
    private SurfaceTextureHelper texture;
    private VideoSource videoSource;
    private AudioSource audioSource;
    private VideoTrack videoTrack;
    private AudioTrack audioTrack;
    private VideoTrack remoteTrack;
    private boolean localDescriptionSent;
    private boolean remoteDescriptionSet;
    private volatile boolean closed;
    private volatile boolean localRendered;
    private volatile boolean remoteRendered;

    public WebRtcPeer(Context context) {
        this.context = context.getApplicationContext();
        local = new SurfaceViewRenderer(context);
        remote = new SurfaceViewRenderer(context);
        local.init(egl.getEglBaseContext(), new RendererCommon.RendererEvents() {
            public void onFirstFrameRendered() { localRendered = true; }
            public void onFrameResolutionChanged(int width, int height, int rotation) {}
        });
        local.setMirror(true);
        remote.init(egl.getEglBaseContext(), new RendererCommon.RendererEvents() {
            public void onFirstFrameRendered() { remoteRendered = true; }
            public void onFrameResolutionChanged(int width, int height, int rotation) {}
        });
        local.setEnableHardwareScaler(true);
        remote.setEnableHardwareScaler(true);
    }

    public android.view.View localView() { return local; }
    public android.view.View remoteView() { return remote; }
    public String poll() { return events.poll(); }

    private void enqueue(Runnable action) {
        try {
            executor.execute(() -> {
                try { action.run(); }
                catch (Exception failure) { error(failure.getMessage()); }
            });
        } catch (RejectedExecutionException failure) {
            if (!closed) throw failure;
        }
    }

    public void command(String json) {
        enqueue(() -> {
            try {
                JSONObject message = new JSONObject(json);
                switch (message.getString("command")) {
                    case "start": start(message.getJSONObject("configuration")); break;
                    case "signal": signal(message.getJSONObject("signal")); break;
                    case "stats": if (peer != null && !closed) peer.getStats(this::stats); break;
                    case "close": close(); break;
                    default: throw new IllegalArgumentException("Unknown WebRTC command");
                }
            } catch (Exception error) { error(error.getMessage()); }
        });
    }

    private void emit(JSONObject value) {
        if (!events.offer(value.toString())) {
            events.clear();
            events.offer("{\"type\":\"error\",\"message\":\"WebRTC event queue overflow\"}");
        }
    }

    private void error(String message) {
        try { emit(new JSONObject().put("type", "error").put("message", message)); }
        catch (org.json.JSONException impossible) { throw new IllegalStateException(impossible); }
    }

    private void start(JSONObject configuration) throws Exception {
        if (factory != null || closed) throw new IllegalStateException("WebRTC already started or closed");
        PeerConnectionFactory.initialize(PeerConnectionFactory.InitializationOptions.builder(context)
            .setEnableInternalTracer(false).createInitializationOptions());
        audioModule = JavaAudioDeviceModule.builder(context).createAudioDeviceModule();
        factory = PeerConnectionFactory.builder().setAudioDeviceModule(audioModule)
            .setVideoEncoderFactory(new DefaultVideoEncoderFactory(egl.getEglBaseContext(), true, true))
            .setVideoDecoderFactory(new DefaultVideoDecoderFactory(egl.getEglBaseContext())).createPeerConnectionFactory();
        List<PeerConnection.IceServer> servers = new ArrayList<>();
        JSONArray configuredServers = configuration.getJSONArray("iceServers");
        boolean relay = configuration.getBoolean("relayOnly");
        for (int i = 0; i < configuredServers.length(); i++) {
            JSONObject item = configuredServers.getJSONObject(i);
            JSONArray urls = item.getJSONArray("urls");
            for (int j = 0; j < urls.length(); j++) {
                String url = urls.getString(j);
                if (relay && !url.startsWith("turn:") && !url.startsWith("turns:")) continue;
                PeerConnection.IceServer.Builder server = PeerConnection.IceServer.builder(url);
                if (!item.isNull("username")) server.setUsername(item.getString("username"));
                if (!item.isNull("credential")) server.setPassword(item.getString("credential"));
                servers.add(server.createIceServer());
            }
        }
        PeerConnection.RTCConfiguration rtc = new PeerConnection.RTCConfiguration(servers);
        rtc.sdpSemantics = PeerConnection.SdpSemantics.UNIFIED_PLAN;
        rtc.bundlePolicy = PeerConnection.BundlePolicy.MAXBUNDLE;
        rtc.rtcpMuxPolicy = PeerConnection.RtcpMuxPolicy.REQUIRE;
        rtc.iceTransportsType = relay ? PeerConnection.IceTransportsType.RELAY : PeerConnection.IceTransportsType.ALL;
        peer = factory.createPeerConnection(rtc, new Observer());
        if (peer == null) throw new IllegalStateException("Cannot create native WebRTC peer");
        Camera2Enumerator cameras = new Camera2Enumerator(context);
        String camera = configuration.getString("camera");
        capturer = cameras.createCapturer(camera, new CameraVideoCapturer.CameraEventsHandler() {
            public void onCameraError(String message) { error("Camera: " + message); }
            public void onCameraDisconnected() { if (!closed) error("Camera disconnected"); }
            public void onCameraFreezed(String message) { error("Camera frozen: " + message); }
            public void onCameraOpening(String name) {}
            public void onFirstFrameAvailable() {}
            public void onCameraClosed() { if (!closed) error("Camera closed unexpectedly"); }
        });
        if (capturer == null) throw new IllegalArgumentException("Camera unavailable");
        texture = SurfaceTextureHelper.create("PostIt-WebRTC-camera", egl.getEglBaseContext());
        videoSource = factory.createVideoSource(false);
        capturer.initialize(texture, context, videoSource.getCapturerObserver());
        videoTrack = factory.createVideoTrack("postit-video", videoSource);
        videoTrack.addSink(local);
        audioSource = factory.createAudioSource(new MediaConstraints());
        audioTrack = factory.createAudioTrack("postit-audio", audioSource);
        peer.addTrack(audioTrack, Collections.singletonList("postit"));
        peer.addTrack(videoTrack, Collections.singletonList("postit"));
        capturer.startCapture(640, 480, 24);
        if (configuration.getBoolean("caller")) peer.createOffer(new CreateDescription("offer"), new MediaConstraints());
    }

    private abstract class SdpCallback implements SdpObserver {
        public void onCreateSuccess(SessionDescription description) {}
        public void onSetSuccess() {}
        public void onCreateFailure(String message) { error("SDP creation failed: " + message); }
        public void onSetFailure(String message) { error("SDP application failed: " + message); }
    }

    private final class CreateDescription extends SdpCallback {
        private final String kind;
        CreateDescription(String kind) { this.kind = kind; }
        public void onCreateSuccess(SessionDescription description) {
            enqueue(() -> {
                if (closed) return;
                peer.setLocalDescription(new SdpCallback() {
                    public void onSetSuccess() {
                        enqueue(() -> {
                            if (closed) return;
                            try {
                                emit(new JSONObject().put("type", "signal").put("signal",
                                    new JSONObject().put("kind", kind).put("sdp", description.description)));
                                localDescriptionSent = true;
                                for (IceCandidate candidate : pendingLocal) sendCandidate(candidate);
                                pendingLocal.clear();
                            } catch (Exception failure) { error(failure.getMessage()); }
                        });
                    }
                }, description);
            });
        }
    }

    private void signal(JSONObject signal) throws Exception {
        if (closed || peer == null) throw new IllegalStateException("WebRTC peer not running");
        String kind = signal.getString("kind");
        if (kind.equals("candidate")) {
            IceCandidate candidate = new IceCandidate(signal.getString("sdpMid"),
                signal.getInt("sdpMLineIndex"), signal.getString("candidate"));
            if (remoteDescriptionSet) addCandidate(candidate);
            else {
                if (pendingRemote.size() >= 256) throw new IllegalStateException("Too many pending ICE candidates");
                pendingRemote.add(candidate);
            }
        } else if (kind.equals("offer") || kind.equals("answer")) {
            peer.setRemoteDescription(new SdpCallback() {
                public void onSetSuccess() {
                    enqueue(() -> {
                        if (closed) return;
                        remoteDescriptionSet = true;
                        for (IceCandidate candidate : pendingRemote) addCandidate(candidate);
                        pendingRemote.clear();
                        if (kind.equals("offer")) peer.createAnswer(new CreateDescription("answer"), new MediaConstraints());
                    });
                }
            }, new SessionDescription(kind.equals("offer") ? SessionDescription.Type.OFFER : SessionDescription.Type.ANSWER,
                signal.getString("sdp")));
        } else throw new IllegalArgumentException("Invalid WebRTC signal");
    }

    private void addCandidate(IceCandidate candidate) {
        peer.addIceCandidate(candidate, new AddIceObserver() {
            public void onAddSuccess() {}
            public void onAddFailure(String message) { error("ICE candidate rejected: " + message); }
        });
    }

    private void sendCandidate(IceCandidate candidate) {
        try {
            emit(new JSONObject().put("type", "signal").put("signal", new JSONObject()
                .put("kind", "candidate").put("candidate", candidate.sdp)
                .put("sdpMid", candidate.sdpMid).put("sdpMLineIndex", candidate.sdpMLineIndex)));
        } catch (Exception failure) { error(failure.getMessage()); }
    }

    private void stats(RTCStatsReport report) {
        enqueue(() -> {
            if (closed) return;
            try {
                Map<String, RTCStats> all = report.getStatsMap();
                String selectedId = null, localType = null;
                long audioPackets = 0, videoFrames = 0;
                for (RTCStats item : all.values()) {
                    Map<String, Object> values = item.getMembers();
                    if (item.getType().equals("transport") && values.get("selectedCandidatePairId") instanceof String)
                        selectedId = (String)values.get("selectedCandidatePairId");
                    if (item.getType().equals("inbound-rtp")) {
                        String media = String.valueOf(values.getOrDefault("kind", values.get("mediaType")));
                        if (media.equals("audio")) audioPackets += number(values.get("packetsReceived"));
                        if (media.equals("video")) videoFrames += number(values.get("framesDecoded"));
                    }
                }
                RTCStats pair = all.get(selectedId);
                if (pair != null) {
                    RTCStats candidate = all.get(String.valueOf(pair.getMembers().get("localCandidateId")));
                    if (candidate != null) localType = String.valueOf(candidate.getMembers().get("candidateType"));
                }
                String state = peer.connectionState().toString().toLowerCase(Locale.ROOT);
                if (state.equals("connected") && localType == null) state = "checking";
                emit(new JSONObject().put("type", "status").put("status", new JSONObject()
                    .put("connectionState", state).put("localCandidateType", localType)
                    .put("audioPackets", audioPackets).put("videoFrames", videoFrames)
                    .put("localVideoRendered", localRendered).put("remoteVideoRendered", remoteRendered)));
            } catch (Exception failure) { error(failure.getMessage()); }
        });
    }

    private static long number(Object value) { return value instanceof Number ? ((Number)value).longValue() : 0; }

    private final class Observer implements PeerConnection.Observer {
        public void onSignalingChange(PeerConnection.SignalingState state) {}
        public void onIceConnectionChange(PeerConnection.IceConnectionState state) {}
        public void onIceConnectionReceivingChange(boolean receiving) {}
        public void onIceGatheringChange(PeerConnection.IceGatheringState state) {}
        public void onIceCandidatesRemoved(IceCandidate[] candidates) {}
        public void onAddStream(MediaStream stream) {}
        public void onRemoveStream(MediaStream stream) {}
        public void onDataChannel(DataChannel channel) {}
        public void onRenegotiationNeeded() {}
        public void onIceCandidate(IceCandidate candidate) {
            enqueue(() -> {
                if (closed) return;
                if (localDescriptionSent) sendCandidate(candidate);
                else if (pendingLocal.size() < 256) pendingLocal.add(candidate);
                else error("Too many local ICE candidates");
            });
        }
        public void onTrack(RtpTransceiver transceiver) {
            MediaStreamTrack track = transceiver.getReceiver().track();
            if (track instanceof VideoTrack) {
                enqueue(() -> {
                    if (closed) return;
                    remoteTrack = (VideoTrack)track;
                    remoteTrack.addSink(remote);
                });
            }
        }
    }

    private interface Cleanup { void run() throws Exception; }

    private void release(Cleanup cleanup, List<String> failures) {
        try { cleanup.run(); }
        catch (Exception failure) { failures.add(failure.toString()); }
    }

    private void close() {
        if (closed) return;
        closed = true;
        List<String> failures = new ArrayList<>();
        if (capturer != null) release(() -> capturer.stopCapture(), failures);
        if (remoteTrack != null) release(() -> remoteTrack.removeSink(remote), failures);
        if (videoTrack != null) release(() -> videoTrack.removeSink(local), failures);
        if (peer != null) {
            release(() -> peer.close(), failures);
            release(() -> peer.dispose(), failures);
        }
        if (videoTrack != null) release(() -> videoTrack.dispose(), failures);
        if (audioTrack != null) release(() -> audioTrack.dispose(), failures);
        if (capturer != null) release(() -> capturer.dispose(), failures);
        if (texture != null) release(() -> texture.dispose(), failures);
        if (videoSource != null) release(() -> videoSource.dispose(), failures);
        if (audioSource != null) release(() -> audioSource.dispose(), failures);
        if (factory != null) release(() -> factory.dispose(), failures);
        if (audioModule != null) release(() -> audioModule.release(), failures);
        new android.os.Handler(android.os.Looper.getMainLooper()).post(() -> {
            release(() -> local.release(), failures);
            release(() -> remote.release(), failures);
            release(() -> egl.release(), failures);
            if (!failures.isEmpty()) error("WebRTC cleanup failed: " + String.join("; ", failures));
            try { emit(new JSONObject().put("type", "closed")); }
            catch (Exception failure) { error(failure.getMessage()); }
        });
        executor.shutdown();
    }
}
