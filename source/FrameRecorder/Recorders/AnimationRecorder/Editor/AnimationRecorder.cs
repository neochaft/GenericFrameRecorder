using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
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
                aInput.gameObjectRecorder.SaveToClip(clip);
                aInput.gameObjectRecorder.ResetRecording();
            }

            ++ars.take;
            base.EndRecording(session);
        }
    }
}