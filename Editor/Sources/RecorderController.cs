using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recorder
{
    public class RecorderController
    {
        readonly SceneHook m_SceneHook;
        
        List<RecordingSession> m_RecordingSessions;
        
        public RecorderController()
        {
            m_SceneHook = new SceneHook(Guid.NewGuid().ToString());
        }
        
        public bool StartRecording(RecorderSettingsPrefs prefs, bool debugMode = false)
        {           
            Debug.Assert(Application.isPlaying);
            
            if (debugMode)
                Debug.Log("Start Recording.");
            
            m_RecordingSessions = new List<RecordingSession>();

            foreach (var recorderSetting in prefs.recorderSettings)
            {
                if (recorderSetting == null)
                {
                    if (debugMode)
                        Debug.Log("Ignoring unknown recorder.");

                    continue;
                }

                prefs.ApplyGlobalSetting(recorderSetting);

                if (recorderSetting.HasErrors())
                {
                    if (debugMode)
                        Debug.Log("Ignoring invalid recorder '" + recorderSetting.name + "'");

                    continue;
                }

                var errors = new List<string>();

                if (recorderSetting.ValidityCheck(errors))
                {
                    foreach (var error in errors)
                        Debug.LogWarning(recorderSetting.name + ": " + error);
                }

                if (errors.Count > 0)
                {
                    if (debugMode)
                        Debug.LogWarning("Recorder '" + recorderSetting.name +
                                         "' has warnings and may not record properly.");
                }

                if (!prefs.IsRecorderEnabled(recorderSetting))
                {
                    if (debugMode)
                        Debug.Log("Ignoring disabled recorder '" + recorderSetting.name + "'");

                    continue;
                }

                var session = m_SceneHook.CreateRecorderSession(recorderSetting, true);

                m_RecordingSessions.Add(session);
            }
            
            var success = m_RecordingSessions.All(r => r.SessionCreated() && r.BeginRecording());

            return success;
        }

        public bool IsRecording()
        {
            return m_RecordingSessions != null && m_RecordingSessions.Any(r => r.isRecording);
        }

        public void StopRecording()
        {
            Debug.Assert(Application.isPlaying);

            if (m_RecordingSessions != null)
            {
                foreach (var recordingSession in m_RecordingSessions)
                    recordingSession.EndRecording();
            }
        }
        
        public IEnumerable<RecordingSession> GetRecordingSessions()
        {
            return m_SceneHook.GetRecordingSessions();
        }
    }
}