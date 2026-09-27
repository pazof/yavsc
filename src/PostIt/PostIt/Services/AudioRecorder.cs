using System;
using System.Threading;
using Pv;

public class AudioRecorder : IDisposable
{
    private int frameLength;
    PvRecorder recorder;

    Thread thread;
    short[] audioFrame;
    IPCMProcessor processor;

    public bool processing { get; private set; }

    public AudioRecorder(IPCMProcessor processor, int frameLength=512, int deviceIndex = -1)
    {
        this.frameLength = frameLength;
        this.recorder = PvRecorder.Create(frameLength: frameLength, deviceIndex: deviceIndex);
        this.audioFrame = new short[frameLength];
        this.processor=processor;
    }

    public static string [] GetAvailableDevices()
    {
        return PvRecorder.GetAvailableDevices();
    }

    public void Start()
    {
        recorder.Start();
        thread = new Thread(ThreadLoop);
        thread.Start();
    }

    public void ThreadLoop()
    {
        while (recorder.IsRecording)
        {
            if (thread.ThreadState != ThreadState.Running)
            {
                break;
            }
            if (processing)
            {
                Thread.Sleep(10);
                continue;
            }
            processing = true;
            audioFrame = recorder.Read();
            processor.Process(audioFrame);
            processing = false;
        }
    }

    public void Stop()
    {
        thread.Interrupt();
        thread.Join();
        recorder.Stop();
        thread = null;
    }
    public void Dispose()
    {
        recorder.Dispose();
    }
}
