using System;
using UnityEngine;
using System.Reflection;
using Unity.Collections;

namespace Recorder.Input
{
    class AudioRenderer
    {
        static MethodInfo m_StartMethod;
        static MethodInfo m_StopMethod;
        static MethodInfo m_GetSampleCountForCaptureFrameMethod;
        static MethodInfo m_RenderMethod;

        static AudioRenderer()
        {
            var className = "UnityEngine.AudioRenderer";
            var dllName = "UnityEngine";
            var audioRecorderType = Type.GetType(className + ", " + dllName);
            if (audioRecorderType == null)
            {
                Debug.Log("AudioInput could not find " + className + " type in " + dllName);
                return;
            }
            m_StartMethod = audioRecorderType.GetMethod("Start");
            m_StopMethod = audioRecorderType.GetMethod("Stop");
            m_GetSampleCountForCaptureFrameMethod =
                audioRecorderType.GetMethod("GetSampleCountForCaptureFrame");
            m_RenderMethod = audioRecorderType.GetMethod("Render");
        }

        public static void Start()
        {
            m_StartMethod.Invoke(null, null);
        }

        public static void Stop()
        {
            m_StopMethod.Invoke(null, null);
        }

        public static uint GetSampleCountForCaptureFrame()
        {
            var count = (int)m_GetSampleCountForCaptureFrameMethod.Invoke(null, null);
            return (uint)count;
        }

        public static void Render(NativeArray<float> buffer)
        {
            m_RenderMethod.Invoke(null, new object[] { buffer });
        }
    }

    public class AudioInput : RecorderInput
    {
        class BufferManager : IDisposable
        {
            readonly NativeArray<float>[] m_Buffers;

            public BufferManager(ushort bufferCount, uint sampleFrameCount, ushort channelCount)
            {
                m_Buffers = new NativeArray<float>[bufferCount];
                for (int i = 0; i < m_Buffers.Length; ++i)
                    m_Buffers[i] = new NativeArray<float>((int)sampleFrameCount * (int)channelCount, Allocator.Temp);
            }

            public NativeArray<float> GetBuffer(int index)
            {
                return m_Buffers[index];
            }

            public void Dispose()
            {
                foreach (var a in m_Buffers)
                    a.Dispose();
            }
        }

        public ushort channelCount { get { return m_ChannelCount; } }
        ushort m_ChannelCount;
        public int sampleRate { get { return AudioSettings.outputSampleRate; } }
        public NativeArray<float> mainBuffer { get { return m_BufferManager.GetBuffer(0); } }
        public NativeArray<float> GetMixerGroupBuffer(int n)
        { return m_BufferManager.GetBuffer(n + 1); }

        BufferManager m_BufferManager;

        public AudioInputSettings audioSettings
        { get { return (AudioInputSettings)settings; } }

        public override void BeginRecording(RecordingSession session)
        {
            m_ChannelCount = new Func<ushort>(() => {
                    switch (AudioSettings.speakerMode)
                    {
                    case AudioSpeakerMode.Mono:        return 1;
                    case AudioSpeakerMode.Stereo:      return 2;
                    case AudioSpeakerMode.Quad:        return 4;
                    case AudioSpeakerMode.Surround:    return 5;
                    case AudioSpeakerMode.Mode5point1: return 6;
                    case AudioSpeakerMode.Mode7point1: return 7;
                    case AudioSpeakerMode.Prologic:    return 2;
                    default: return 1;
                    }
            })();

            if (Options.debugMode)
                Debug.Log(string.Format(
                              "AudioInput.BeginRecording for capture frame rate {0}", Time.captureFramerate));

            if (audioSettings.preserveAudio)
                AudioRenderer.Start();
        }

        public override void NewFrameReady(RecordingSession session)
        {
            if (!audioSettings.preserveAudio)
                return;

            var sampleFrameCount = (uint)AudioRenderer.GetSampleCountForCaptureFrame();
            if (Options.debugMode)
                Debug.Log(string.Format("AudioInput.NewFrameReady {0} audio sample frames @ {1} ch",
                                        sampleFrameCount, m_ChannelCount));

            const ushort bufferCount = 1;

            m_BufferManager = new BufferManager(bufferCount, sampleFrameCount, m_ChannelCount);
            var mainBuffer = m_BufferManager.GetBuffer(0);

            AudioRenderer.Render(mainBuffer);
        }

        public override void FrameDone(RecordingSession session)
        {
            if (!audioSettings.preserveAudio)
                return;

            m_BufferManager.Dispose();
            m_BufferManager = null;
        }

        public override void EndRecording(RecordingSession session)
        {
            if (audioSettings.preserveAudio)
                AudioRenderer.Stop();
        }
    }
}