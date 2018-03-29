using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Experimental.Recorder
{
    [Serializable]
    public class AnimationRecorderSettings : RecorderSettings
    {
        AnimationRecorderSettings()
        {
            baseFileName.pattern = "animation_<0000>.anim";
        }
        
        public override List<RecorderInputSetting> GetDefaultInputSettings()
        {
            return new List<RecorderInputSetting>
            {
                NewInputSettingsObj<AnimationInputSettings>() 
            };
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

        public override InputGroups GetInputGroups()
        {
            return new InputGroups
            {
                new List<Type>
                {
                    typeof(AnimationInputSettings)
                }
            };
        }

        public override bool ValidityCheck( List<string> errors )
        {
            var ok = base.ValidityCheck(errors);

            if (inputsSettings == null)
            {
                ok = false;
                errors.Add("Invalid state!");
            }
            
            if (!inputsSettings.Cast<AnimationInputSettings>().Any(x => x != null && x.enabled && x.gameObject != null ))
            {
                ok = false;
                errors.Add("No input object set/enabled.");
            }

            return ok; 
        }
    }
}