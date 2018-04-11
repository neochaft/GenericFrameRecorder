using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.Recorder
{  
    public static class SceneHook
    {
        const string k_HostGoName = "UnityEngine-Recorder";

        public static GameObject GetRecorderHost(bool createIfAbsent = false)
        {
            var go = GameObject.Find(k_HostGoName);
            /*if (go != null && go.scene != SceneManager.GetActiveScene())
                go = null;*/

            if (go == null && createIfAbsent)
            {
                go = new GameObject(k_HostGoName);
                //if (!Verbose.enabled)
                //    go.hideFlags = HideFlags.HideInHierarchy;
            }
            else if (go != null)
            {
                go.hideFlags = HideFlags.None; //HideInHierarchy;
            }

            return go;
        }

        public static RecorderBindings GetRecorderBindings()
        {
            var go = GetRecorderHost(true);
            var rb = go.GetComponent<RecorderBindings>();
            if (rb == null)
                rb = go.AddComponent<RecorderBindings>();

            return rb;
        }

        public static IEnumerable<RecordingSession> GetCurrentRecordingSessions()
        {
            var host = GetRecorderHost();
            if (host != null)
            {
                var components = host.GetComponents<RecorderComponent>();
                foreach (var component in components)
                {
                    yield return component.session;
                }      
            }
        }
        
        public static RecordingSession CreateRecorderSession(RecorderSettings settings, bool autoExitPlayMode)
        {
            var component = GetRecorderComponent(settings, true);
            
            var session = new RecordingSession
            {
                m_Recorder = RecordersInventory.GenerateNewRecorder(settings.recorderType, settings),
                m_RecorderGO = component.gameObject
            };
         
            component.autoExitPlayMode = autoExitPlayMode;
            component.session = session;

            return session;
        }
        
        static RecorderComponent GetRecorderComponent(RecorderSettings settings, bool createIfAbsent)
        {
            var host = GetRecorderHost(createIfAbsent);
            if (host == null)
                return null;

            var component = host.GetComponentsInChildren<RecorderComponent>().FirstOrDefault(r => r.session.settings == settings);

            if (component == null && createIfAbsent)
                component = host.AddComponent<RecorderComponent>();

            return component;
        }
    }
}
