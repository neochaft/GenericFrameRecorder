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
            baseFileName.pattern = "animation_<0000>.anim";
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

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if (!m_AnimationInputSettings.enabled || m_AnimationInputSettings.gameObject == null)
            {
                ok = false;
                errors.Add("No input object set/enabled.");
            }

            return ok; 
        }
    }
}