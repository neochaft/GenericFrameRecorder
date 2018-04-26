using UnityEditor.Experimental.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Experimental.Recorder
{
    [Recorder(typeof(AnimationRecorderSettings), "Animation", "animation_recorder")]
    public class AnimationRecorder : GenericRecorder<AnimationRecorderSettings>
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
                var clip = new AnimationClip();
                var clipName = ars.fileNameGenerator.BuildFileName(session);
                
                ars.fileNameGenerator.path.CreateDirectory();
                clipName = "Assets/" + ars.fileNameGenerator.path.leaf + "/" + AssetDatabase.GenerateUniqueAssetPath(clipName);
                AssetDatabase.CreateAsset(clip, clipName);
                aInput.m_gameObjectRecorder.SaveToClip(clip);
                aInput.m_gameObjectRecorder.ResetRecording();
            }

            ++ars.take;
            base.EndRecording(session);
        }
    }
}