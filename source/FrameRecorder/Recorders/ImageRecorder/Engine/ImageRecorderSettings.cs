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
            m_BaseFileName.pattern = "image_<0000>.<ext>";
        }

        public override List<RecorderInputSetting> GetDefaultInputSettings()
        {
            return new List<RecorderInputSetting>
            {
                NewInputSettingsObj<CBRenderTextureInputSettings>("Pixels") 
            };
        }

        public override bool ValidityCheck( List<string> errors )
        {
            var ok = base.ValidityCheck(errors);

            if( string.IsNullOrEmpty(m_DestinationPath.GetFullPath() ))
            {
                ok = false;
                errors.Add("Missing destination path.");
            } 
            if(  string.IsNullOrEmpty(m_BaseFileName.pattern))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }

        public override bool SelfAdjustSettings()
        {
            if (inputsSettings.Count == 0 )
                return false;

            bool adjusted = false;

            var input = inputsSettings[0] as RenderTextureSamplerSettings;
            if (input != null)
            {
                var colorSpace = m_OutputFormat == ImageRecorderOutputFormat.EXR ? ColorSpace.Linear : ColorSpace.Gamma;
                if (input.m_ColorSpace != colorSpace)
                {
                    input.m_ColorSpace = colorSpace;
                    adjusted = true;
                }
            }

            var iis = inputsSettings[0] as ImageInputSettings;
            if (iis != null)
            {
                if (iis.maxSupportedSize != EImageDimension.x4320p_8K)
                {
                    iis.maxSupportedSize = EImageDimension.x4320p_8K;
                    adjusted = true;
                }
            }

            return adjusted;
        }

        public override List<InputGroupFilter> GetInputGroups()
        {
            return new List<InputGroupFilter>()
            {
                new InputGroupFilter()
                {
                    title = "Pixels", typesFilter = new List<InputFilter>()
                    {
#if UNITY_2017_3_OR_NEWER
                        new TInputFilter<ScreenCaptureInputSettings>("Screen"),
#endif
                        new TInputFilter<CBRenderTextureInputSettings>("Camera(s)"),
#if UNITY_2018_1_OR_NEWER
                        new TInputFilter<Camera360InputSettings>("360 View (feature preview)"),
#endif
                        new TInputFilter<RenderTextureSamplerSettings>("Sampling"),
                        new TInputFilter<RenderTextureInputSettings>("Render Texture"),
                    }
                }
            };
        }
    }
}
