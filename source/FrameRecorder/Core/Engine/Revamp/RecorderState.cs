using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recorder
{
    public class RecorderState
    {
        //SceneHook m_SceneHook;

//        public RecorderState()
//        {
//            m_SceneHook = new SceneHook(ObjectNames.GetUniqueName(new [] { "Recorder SceneHook" } , "Recorder SceneHook"));
//        }
        
        public bool StartRecording(RecorderSettingsPrefs prefs, bool debugMode = false)
        {           
            Debug.Assert(Application.isPlaying);
            
            if (debugMode)
                Debug.Log("Start Recording.");
            
            var sessions = new List<RecordingSession>();

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

                var session = SceneHook.CreateRecorderSession(recorderSetting, true);

                sessions.Add(session);
            }
            
            var success = sessions.All(s => s.SessionCreated() && s.BeginRecording());

            return success;
        }

        public void StopRecording()
        {
            Debug.Assert(Application.isPlaying);
            
            var recorderGO = SceneHook.GetRecorderHost();
            if (recorderGO != null)
            {
                UnityHelpers.Destroy(recorderGO);
            }
        }
        
        public IEnumerable<RecordingSession> GetRecordingSessions()
        {
            return SceneHook.GetCurrentRecordingSessions();
        }
    }
}