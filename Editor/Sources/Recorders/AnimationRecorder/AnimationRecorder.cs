using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    class AnimationRecorder : GenericRecorder<AnimationRecorderSettings>
    {
        public override void RecordFrame(RecordingSession session)
        {
        }

        public override void EndRecording(RecordingSession session)
        {
            var ars = (AnimationRecorderSettings)session.settings;

            foreach (var input in m_Inputs)
            {
                var aInput = (AnimationInput)input;

                if (aInput.gameObjectRecorder == null)
                    continue;
                
                var clip = new AnimationClip();
                
                ars.fileNameGenerator.CreateDirectory(session);
                
                var clipName = ars.fileNameGenerator.BuildAbsolutePath(session).Replace(Application.dataPath, "Assets");
                
                AssetDatabase.CreateAsset(clip, clipName);
                aInput.gameObjectRecorder.SaveToClip(clip);
                aInput.gameObjectRecorder.ResetRecording();
            }

            base.EndRecording(session);
        }
    }
}