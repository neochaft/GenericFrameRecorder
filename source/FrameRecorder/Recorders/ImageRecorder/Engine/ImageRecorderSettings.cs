using System;
using System.Collections.Generic;
using UnityEngine.Recorder.Input;

namespace UnityEngine.Recorder
{
    public enum ImageRecorderOutputFormat
    {
        PNG,
        JPEG,
        EXR
    }

    public class ImageRecorderSettings : RecorderSettings
    {
        public ImageRecorderOutputFormat m_OutputFormat = ImageRecorderOutputFormat.JPEG;
        public bool m_IncludeAlpha;

        ImageRecorderSettings()
        {
            baseFileName.pattern = "image_<0000>.<ext>";
        }

        public override List<RecorderInputSetting> GetDefaultInputSettings()
        {
            return new List<RecorderInputSetting>
            {
                NewInputSettingsObj<CBRenderTextureInputSettings>() 
            };
        }

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

        public override void SelfAdjustSettings()
        {
            if (inputsSettings.Count == 0 )
                return;

            var input = inputsSettings[0] as RenderTextureSamplerSettings;
            if (input != null)
            {
                var colorSpace = m_OutputFormat == ImageRecorderOutputFormat.EXR ? ColorSpace.Linear : ColorSpace.Gamma;
                input.colorSpace = colorSpace;
            }

            var iis = inputsSettings[0] as ImageInputSettings;
            if (iis != null)
            {
                iis.maxSupportedSize = EImageDimension.x4320p_8K;
            }
        }

        public override InputGroups GetInputGroups()
        {
            return new InputGroups
            {
                new List<Type>
                {
                    typeof(ScreenCaptureInputSettings),
                    typeof(CBRenderTextureInputSettings),
                    typeof(Camera360InputSettings),
                    typeof(RenderTextureInputSettings),
                    typeof(RenderTextureSamplerSettings)
                }
            };
        }
    }
}
