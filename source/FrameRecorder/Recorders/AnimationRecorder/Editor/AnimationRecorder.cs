using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

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
            var ars = session.settings as AnimationRecorderSettings;

            foreach (var input in m_Inputs)
            {
                var aInput = input as AnimationInput;
                AnimationClip clip = new AnimationClip();
                var clipName = ars.fileNameGenerator.BuildFileName(session);
                
                ars.fileNameGenerator.path.CreateDirectory();
                clipName = "Assets/" + ars.fileNameGenerator.path.leaf + "/" + AssetDatabase.GenerateUniqueAssetPath(clipName);
                AssetDatabase.CreateAsset(clip, clipName);
                aInput.m_gameObjectRecorder.SaveToClip(clip);
                aInput.m_gameObjectRecorder.ResetRecording();
            }

            base.EndRecording(session);
        }
    }
}