using System;
using System.Collections;

namespace UnityEngine.Recorder
{

    /// <summary>
    /// What is this: 
    /// Motivation  : 
    /// Notes: 
    /// </summary>    
    [ExecuteInEditMode]
    public class RecorderComponent : MonoBehaviour
    {
        public bool autoExitPlayMode { get; set; }
        public RecordingSession session { get; set; }

        public void Update()
        {
            if (session != null && session.recording)
                session.PrepareNewFrame();
        }

        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();
            if (session != null && session.recording)
            {
                session.RecordFrame();

                switch (session.m_Recorder.settings.recordMode)
                {
                    case RecordMode.Manual:
                        break;
                    case RecordMode.SingleFrame:
                    {
                        if (session.m_Recorder.recordedFramesCount == 1)
                            enabled = false;
                        break;
                    }
                    case RecordMode.FrameInterval:
                    {
                        if (session.frameIndex > session.settings.endFrame)
                            enabled = false;
                        break;
                    }
                    case RecordMode.TimeInterval:
                    {
                        if (RecorderSettings.frameRatePlayback == FrameRatePlayback.Variable)
                        {
                            if (session.m_CurrentFrameStartTS >= session.settings.endTime)
                                enabled = false;
                        }
                        else
                        {
                            var expectedFrames = (session.settings.endTime - session.settings.startTime) * session.settings.frameRate;
                            if (session.RecordedFrameSpan >= expectedFrames)
                                enabled = false;
                        }
                        break;
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (session != null && session.recording)
                StartCoroutine(RecordFrame());
        }

        public void OnDisable()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;

#if UNITY_EDITOR
                if (autoExitPlayMode)
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        public void OnDestroy()
        {
            if (session != null)
                session.Dispose();
        }
    }
}
