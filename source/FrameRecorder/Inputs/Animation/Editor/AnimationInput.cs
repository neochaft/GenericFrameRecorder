using System;
using UnityEditor.Experimental.Animations;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Experimental.Recorder.Input
{
    public class AnimationInput : RecorderInput
    {
        public GameObjectRecorder m_gameObjectRecorder;
        private float m_time;

        public override void BeginRecording(RecordingSession session)
        {
            var aniSettings = (AnimationInputSettings) settings;

            var srcGO = aniSettings.gameObject.Resolve(SceneHook.GetRecorderBindings()) ;

            m_gameObjectRecorder = new GameObjectRecorder(srcGO);

            foreach (var binding in aniSettings.bindingType)
            {
                m_gameObjectRecorder.BindComponent(srcGO, binding, aniSettings.recursive); 
            }
            
            m_time = session.recorderTime;
        }

        public override void NewFrameReady(RecordingSession session)
        {
            if (session.recording)
            {
                m_gameObjectRecorder.TakeSnapshot(session.recorderTime - m_time);
                m_time = session.recorderTime;
            }
        }        
    }
}