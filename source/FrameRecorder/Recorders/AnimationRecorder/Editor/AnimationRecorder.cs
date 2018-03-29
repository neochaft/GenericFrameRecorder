using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Experimental.Recorder
{
    [Recorder(typeof(AnimationRecorderSettings), "Animation Clips", "Unity/Animation Recording", "animation_recorder")]
    public class AnimationRecorder : GenericRecorder<AnimationRecorderSettings>
    {
        public override void RecordFrame(RecordingSession session)
        {
        }


        public override void EndRecording(RecordingSession session)
        {
            var ars = session.settings as AnimationRecorderSettings;

            for (int i = 0; i < m_Inputs.Count; ++i)
            {
                var set = (AnimationInputSettings)settings.inputsSettings[i];
                if (set.enabled)
                {
                    var aInput = m_Inputs[i] as AnimationInput;
                    AnimationClip clip = new AnimationClip();
                    var clipName = ars.baseFileName.BuildFileName(session, recordedFramesCount, 0, 0, "anim");
                    
                    ars.destinationPath.CreateDirectory();
                    clipName = "Assets/" + ars.destinationPath.leaf + "/" + AssetDatabase.GenerateUniqueAssetPath(clipName);
                    AssetDatabase.CreateAsset(clip, clipName);
                    aInput.m_gameObjectRecorder.SaveToClip(clip);
                    aInput.m_gameObjectRecorder.ResetRecording();
                }
            }

            base.EndRecording(session);
        }
    }
}