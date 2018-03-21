using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UTJ.FrameCapturer.Recorders
{
    public abstract class BaseFCRecorderSettings : RecorderSettings
    {
        public override bool ValidityCheck( List<string> errors )
        {
            var ok = base.ValidityCheck(errors);
            
            if( string.IsNullOrEmpty(destinationPath.GetFullPath() ))
            {
                ok = false;
                errors.Add("Missing destination path.");
            } 
            if(  string.IsNullOrEmpty(baseFileName.pattern))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || 
                       Application.platform == RuntimePlatform.WindowsPlayer ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.OSXPlayer ||
                       Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.LinuxPlayer;
            }
        }

        public override RecorderInputSetting NewInputSettingsObj(Type type)
        {
            var obj = base.NewInputSettingsObj(type);
            if (type == typeof(CBRenderTextureInputSettings))
            {
                var settings = (CBRenderTextureInputSettings)obj;
                settings.flipFinalOutput = true;
            }
            else if (type == typeof(RenderTextureSamplerSettings))
            {
                var settings = (RenderTextureSamplerSettings)obj;
                settings.flipFinalOutput = true;
            }

            return obj ;
        }

        public override InputGroups GetInputGroups()
        {
            return new InputGroups
            {
                new List<Type>
                {
                    typeof(CBRenderTextureInputSettings),
                    typeof(RenderTextureSamplerSettings),
                    typeof(RenderTextureInputSettings)
                }
            };
        }

        public override void SelfAdjustSettings()
        {
            if (inputsSettings.Count == 0 )
                return;

            var iis = inputsSettings[0] as ImageInputSettings;
            if (iis != null)
            {
                iis.maxSupportedSize = EImageDimension.x4320p_8K;
            }
        }


    }
}
