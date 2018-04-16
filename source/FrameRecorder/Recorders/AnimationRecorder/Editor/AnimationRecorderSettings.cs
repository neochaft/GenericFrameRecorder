using System;
using System.Collections.Generic;
using UnityEditor.Experimental.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Experimental.Recorder
{
    [Serializable]
    public class AnimationRecorderSettings : RecorderSettings
    {
        [SerializeField] AnimationInputSettings m_AnimationInputSettings = new AnimationInputSettings();

        public AnimationRecorderSettings()
        {
            fileNameGenerator.pattern = "animation_" + FileNameGenerator.GetTagPattern(ETags.Frame);
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.WindowsEditor;
            }
        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get { yield return m_AnimationInputSettings; }
        }

        public override string extension
        {
            get { return "anim"; }
        }

        public override Vector2 resolution
        {
            get { return Vector2.zero; }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if (m_AnimationInputSettings.gameObject.Resolve(SceneHook.GetRecorderBindings()) == null)
            {
                ok = false;
                errors.Add("No input object set/enabled.");
            }

            return ok; 
        }
        
        public override void OnAfterDuplicate()
        {
            m_AnimationInputSettings.DuplicateExposedReference();
        }
        
        void OnDestroy()
        {
            m_AnimationInputSettings.ClearExposedReference();
        }
    }
}