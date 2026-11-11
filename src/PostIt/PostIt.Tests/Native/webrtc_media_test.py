"""Real GStreamer/coturn regression. No camera, account or public service is used."""
import base64
import hashlib
import hmac
import io
import json
import os
from pathlib import Path
import queue
import secrets
import shutil
import socket
import subprocess
import sys
import tempfile
import threading
import time
import unittest

ENGINE = Path(__file__).resolve().parents[2] / "PostIt/Services/WebRtc/gstreamer_peer.py"


class AndroidPeer:
    """Debug Activity over an adb-reversed, loopback-only test command socket."""
    def __init__(self):
        self.adb = [os.environ.get("POSTIT_ADB", "adb")]
        if os.environ.get("POSTIT_ANDROID_SERIAL"):
            self.adb += ["-s", os.environ["POSTIT_ANDROID_SERIAL"]]
        self.exited = threading.Event()
        self.reversed = []
        with socket.socket() as listener:
            listener.bind(("127.0.0.1", 0))
            listener.listen(1)
            listener.settimeout(35)
            port = listener.getsockname()[1]
            self.reverse(port)
            try:
                subprocess.run(self.adb + ["shell", "am", "start", "-f", "0x18000000", "-n",
                    "fr.pschneider.postit/.WebRtcTestActivity", "--ei", "port", str(port)],
                    check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, timeout=10)
                self.socket, _ = listener.accept()
            except BaseException:
                self.remove_reverses()
                raise
        self.input = self.socket.makefile("w", encoding="utf-8")
        self.output = self.socket.makefile("r", encoding="utf-8")
        self.stdin = self.input
        self.stdout = self.read()
        self.stderr = io.StringIO()

    def reverse(self, port):
        subprocess.run(self.adb + ["reverse", f"tcp:{port}", f"tcp:{port}"], check=True,
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=10)
        self.reversed.append(port)

    def remove_reverses(self):
        for port in self.reversed:
            subprocess.run(self.adb + ["reverse", "--remove", f"tcp:{port}"], check=True,
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=10)
        self.reversed.clear()

    def read(self):
        try:
            yield from self.output
        finally:
            self.output.close()
            self.exited.set()

    def poll(self):
        return 0 if self.exited.is_set() else None

    def wait(self, timeout=None):
        if not self.exited.wait(timeout):
            raise subprocess.TimeoutExpired("Android test peer", timeout)
        self.socket.close()
        self.remove_reverses()
        return 0

    def kill(self):
        self.socket.shutdown(socket.SHUT_RDWR)
        self.socket.close()


class NativeWebRtcTests(unittest.TestCase):
    def run_pair(self, relay, unavailable=False, android=False, android_caller=False):
        processes = []
        events = queue.Queue()
        errors = []
        server = None
        server_log = None
        with tempfile.TemporaryDirectory(prefix="postit-turn-") as directory:
            try:
                servers = []
                if relay:
                    binary = os.environ.get("POSTIT_TURN_SERVER", shutil.which("turnserver"))
                    self.assertTrue(binary, "Install coturn or set POSTIT_TURN_SERVER")
                    with socket.socket() as reservation:
                        reservation.bind(("127.0.0.1", 0))
                        port = reservation.getsockname()[1]
                    secret = secrets.token_hex(32)
                    username = str(int(time.time()) + 600) + ":native-test"
                    password = base64.b64encode(hmac.new(secret.encode(), username.encode(), hashlib.sha1).digest()).decode()
                    server_log = open(Path(directory) / "coturn.log", "w+")
                    server = subprocess.Popen([
                        binary, "-n", "--listening-ip=127.0.0.1", "--relay-ip=127.0.0.1",
                        "--listening-port=" + str(port), "--min-port=52000", "--max-port=52100",
                        "--realm=postit-native-test", "--use-auth-secret", "--static-auth-secret=" + secret,
                        "--allow-loopback-peers", "--no-multicast-peers", "--no-tls", "--no-dtls",
                        "--no-cli", "--relay-threads=1", "--pidfile=" + directory + "/turn.pid",
                        "--log-file=stdout"
                    ], stdout=server_log, stderr=subprocess.STDOUT)
                    deadline = time.monotonic() + 10
                    while True:
                        try:
                            with socket.create_connection(("127.0.0.1", port), timeout=0.2):
                                break
                        except OSError:
                            if time.monotonic() > deadline or server.poll() is not None:
                                server_log.seek(0)
                                self.fail("coturn failed to start: " + server_log.read()[-2000:])
                            time.sleep(0.1)
                    if unavailable:
                        server.terminate()
                        server.wait(timeout=5)
                    servers = [{"urls": [f"turn:127.0.0.1:{port}?transport=tcp"],
                                "username": username, "credential": password}]

                def read(index, process):
                    for line in process.stdout:
                        try:
                            message = json.loads(line)
                            if message.get("type") == "frame":
                                events.put((index, {"type": "rendered", "target": message["target"],
                                                    "size": len(base64.b64decode(message["data"]))}))
                            else:
                                events.put((index, message))
                        except Exception as error:
                            errors.append(str(error))

                def stderr(process):
                    for line in process.stderr:
                        errors.append(line.rstrip())

                for index in range(2):
                    if android and index == 1:
                        process = AndroidPeer()
                        if relay:
                            process.reverse(port)
                    else:
                        process = subprocess.Popen([sys.executable, "-X", "faulthandler", "-u", str(ENGINE)],
                                                   stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                                   stderr=subprocess.PIPE, text=True, bufsize=1)
                    processes.append(process)
                    threading.Thread(target=read, args=(index, process), daemon=True).start()
                    threading.Thread(target=stderr, args=(process,), daemon=True).start()
                    self.send(process, {"command": "start", "configuration": {
                        "iceServers": servers, "relayOnly": relay,
                        "caller": index == (1 if android_caller else 0),
                        "camera": "test", "microphone": "test"}})
                stats = {}
                rendered = set()
                candidates = 0
                descriptions = set()
                started = time.monotonic()
                deadline = started + (12 if unavailable else 35)
                while time.monotonic() < deadline:
                    self.assertTrue(all(p.poll() is None for p in processes),
                                    f"Native peer exited: {[p.poll() for p in processes]}, {errors[-20:]}")
                    try:
                        index, message = events.get(timeout=0.5)
                    except queue.Empty:
                        self.assertTrue(all(p.poll() is None for p in processes), str(errors[-10:]))
                        continue
                    kind = message["type"]
                    if kind == "error":
                        self.fail(message["message"] + "\n" + str(errors[-10:]))
                    if kind == "signal":
                        signal = message["signal"]
                        if relay:
                            candidate = signal.get("candidate")
                            if candidate:
                                tokens = candidate.split()
                                self.assertEqual("relay", tokens[7])
                                if "raddr" in tokens:
                                    self.assertIn(tokens[tokens.index("raddr") + 1], ("0.0.0.0", "::"))
                            for line in (signal.get("sdp") or "").splitlines():
                                if line.startswith("a=candidate:"):
                                    self.assertEqual("relay", line.split()[7])
                                if line.startswith(("o=", "c=")):
                                    self.assertIn(line.split()[-1], ("0.0.0.0", "::", "127.0.0.1"))
                        if signal["kind"] == "candidate":
                            candidates += 1
                        else:
                            descriptions.add((index, signal["kind"]))
                        self.send(processes[1 - index], {"command": "signal", "signal": signal})
                    if kind == "rendered" and message["size"] > 100:
                        rendered.add((index, message["target"]))
                    if kind == "status":
                        stats[index] = message["status"]
                        if unavailable:
                            self.assertNotEqual("connected", message["status"]["connectionState"])
                            self.assertEqual(0, message["status"]["audioPackets"])
                            self.assertEqual(0, message["status"]["videoFrames"])
                        elif time.monotonic() - started >= 10 and len(stats) == 2 and len(rendered) == 4 and all(
                            s["connectionState"] == "connected" and s["audioPackets"] > 100
                            and s["videoFrames"] >= 100 for s in stats.values()
                        ):
                            if relay:
                                self.assertTrue(all(s["localCandidateType"] == "relay" for s in stats.values()))
                            self.assertGreater(candidates, 0)
                            print(f"\nMedia verified: relay={relay}, android={android}, "
                                  f"androidCaller={android_caller}, stats={stats}, rendered={len(rendered)}",
                                  flush=True)
                            return
                if unavailable:
                    caller = 1 if android_caller else 0
                    self.assertEqual({(caller, "offer"), (1 - caller, "answer")}, descriptions)
                    self.assertEqual({0, 1}, set(stats))
                    self.assertTrue(all((index, "local") in rendered for index in range(2)))
                    self.assertEqual(0, candidates, "TURN failure must never disclose direct candidates")
                    self.assertFalse(any(target == "remote" for _, target in rendered))
                    return
                self.fail(f"No bidirectional decoded/rendered media: {stats}, {rendered}, {errors[-12:]}")
            finally:
                for process in processes:
                    if process.poll() is None:
                        self.send(process, {"command": "close"})
                        try:
                            process.wait(timeout=5)
                        except subprocess.TimeoutExpired:
                            process.kill()
                            process.wait()
                    for stream in (process.stdin, process.stdout, process.stderr):
                        stream.close()
                if server is not None and server.poll() is None:
                    server.terminate()
                    server.wait(timeout=5)
                if server_log is not None:
                    server_log.close()

    @staticmethod
    def send(process, message):
        process.stdin.write(json.dumps(message) + "\n")
        process.stdin.flush()

    def test_direct_bidirectional_media(self):
        self.run_pair(False)

    def test_relay_bidirectional_media_and_no_address_disclosure(self):
        self.run_pair(True)

    def test_turn_unavailable_never_falls_back_to_direct(self):
        self.run_pair(True, unavailable=True)

    @unittest.skipUnless(os.environ.get("POSTIT_TEST_ANDROID_WEBRTC") == "1", "Requires installed Debug APK and adb device")
    def test_android_linux_relay_bidirectional_media(self):
        for caller in (False, True):
            with self.subTest(android_caller=caller):
                self.run_pair(True, android=True, android_caller=caller)

    @unittest.skipUnless(os.environ.get("POSTIT_TEST_ANDROID_WEBRTC") == "1", "Requires installed Debug APK and adb device")
    def test_android_turn_unavailable_never_falls_back_to_direct(self):
        self.run_pair(True, unavailable=True, android=True)


if __name__ == "__main__":
    unittest.main()
