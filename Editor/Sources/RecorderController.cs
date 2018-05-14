using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    public class RecorderController
    {
        readonly SceneHook m_SceneHook;
        
        List<RecordingSession> m_RecordingSessions;
        readonly RecorderSettingsPrefs m_Prefs;

        public RecorderSettingsPrefs prefs
        {
            get { return m_Prefs; }
        }

        public RecorderController(RecorderSettingsPrefs prefs)
        {          
            m_Prefs = prefs;   
            m_SceneHook = new SceneHook(Guid.NewGuid().ToString());
        }

        public bool debugMode;
        
        public bool StartRecording()
        {          
            if (!Application.isPlaying)
                throw new Exception("Start Recording can only be called in Playmode.");

            if (m_Prefs == null)
                throw new NullReferenceException("Can start recording without prefs");
            
            if (IsRecording())
            {
                if (debugMode)
                    Debug.Log("Recording was already started.");
                
                return false;
            }

            if (debugMode)
                Debug.Log("Start Recording.");
            
            m_RecordingSessions = new List<RecordingSession>();

            foreach (var recorderSetting in m_Prefs.recorderSettings)
            {
                if (recorderSetting == null)
                {
                    if (debugMode)
                        Debug.Log("Ignoring unknown recorder.");

                    continue;
                }

                m_Prefs.ApplyGlobalSetting(recorderSetting);

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

                if (!m_Prefs.IsRecorderEnabled(recorderSetting))
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
            if (debugMode)
                Debug.Log("Stop Recording.");

            if (m_RecordingSessions != null)
            {
                foreach (var recordingSession in m_RecordingSessions)
                    recordingSession.EndRecording();

                m_RecordingSessions = null;
            }
        }
        
        internal IEnumerable<RecordingSession> GetRecordingSessions()
        {
            return m_SceneHook.GetRecordingSessions();
        }
    }
}