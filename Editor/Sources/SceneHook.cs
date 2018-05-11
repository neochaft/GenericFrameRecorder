using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{  
    class SceneHook
    {
        const string k_HostGoName = "UnityEngine-Recorder";
        
        static GameObject s_SessionHooksRoot;

        readonly string m_SessionId;
        GameObject m_SessionHook;
        
        public SceneHook(string sessionId)
        {
            m_SessionId = sessionId;
        }

        static GameObject GetSessionHooksRoot()
        {
            if (s_SessionHooksRoot == null)
            {
                s_SessionHooksRoot = GameObject.Find(k_HostGoName);

                if (s_SessionHooksRoot == null)
                    s_SessionHooksRoot = new GameObject(k_HostGoName);
                
                s_SessionHooksRoot.hideFlags = HideFlags.None;
                s_SessionHooksRoot.tag = "EditorOnly";
            }

            return s_SessionHooksRoot;
        }

        public static RecorderBindings GetRecorderBindings()
        {
            var go = GetSessionHooksRoot();
            var rb = go.GetComponent<RecorderBindings>();
            if (rb == null)
                rb = go.AddComponent<RecorderBindings>();

            return rb;
        }

        GameObject GetSessionHook()
        {
            if (m_SessionHook != null)
                return m_SessionHook;
            
            var host = GetSessionHooksRoot();
            if (host == null)
                return null;
            
            m_SessionHook = GameObject.Find(m_SessionId);
            if (m_SessionHook == null)
            {
                m_SessionHook = new GameObject(m_SessionId);
                m_SessionHook.transform.parent = host.transform;   
            }

            return m_SessionHook;
        }

        public IEnumerable<RecordingSession> GetRecordingSessions()
        {
            var sessionHook = GetSessionHook();
            if (sessionHook != null)
            {
                var components = sessionHook.GetComponents<RecorderComponent>();
                foreach (var component in components)
                {
                    yield return component.session;
                }      
            }
        }
        
        public RecordingSession CreateRecorderSession(RecorderSettings settings, bool autoExitPlayMode)
        {
            var component = GetRecorderComponent(settings);
            
            var session = new RecordingSession
            {
                recorder = RecordersInventory.CreateDefaultRecorder(settings),
                recorderGameObject = component.gameObject
            };
         
            component.autoExitPlayMode = autoExitPlayMode;
            component.session = session;

            return session;
        }
        
        RecorderComponent GetRecorderComponent(RecorderSettings settings)
        {
            var sceneHook = GetSessionHook();
            if (sceneHook == null)
                return null;

            var component = sceneHook.GetComponentsInChildren<RecorderComponent>().FirstOrDefault(r => r.session.settings == settings);

            if (component == null)
                component = sceneHook.AddComponent<RecorderComponent>();

            return component;
        }
    }
}
