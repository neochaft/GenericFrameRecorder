using System;
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEngine.Recorder
{   
    public enum ImageRecorderOutputFormat
    {
        PNG,
        JPEG,
        EXR
    }
    
    [Serializable]
    public class VideoSelector : InputSettingsSelector
    {      
        [SerializeField] CBRenderTextureInputSettings m_CbRenderTextureInputSettings = new CBRenderTextureInputSettings();
        [SerializeField] ScreenCaptureInputSettings m_ScreenCaptureInputSettings = new ScreenCaptureInputSettings();
        [SerializeField] Camera360InputSettings m_Camera360InputSettings = new Camera360InputSettings();
        [SerializeField] RenderTextureInputSettings m_RenderTextureInputSettings = new RenderTextureInputSettings();
        [SerializeField] RenderTextureSamplerSettings m_RenderTextureSamplerSettings = new RenderTextureSamplerSettings();
    }
    
    public class ImageRecorderSettings : RecorderSettings
    {
        public ImageRecorderOutputFormat m_OutputFormat = ImageRecorderOutputFormat.JPEG;

        [SerializeField] VideoSelector m_VideoSelector = new VideoSelector();

        public ImageRecorderSettings()
        {
            fileNameGenerator.pattern = "image_<0000>.<ext>";
        }
        
        public override bool ValidityCheck( List<string> errors )
        {
            var ok = base.ValidityCheck(errors);

            if( string.IsNullOrEmpty(fileNameGenerator.path.GetFullPath() ))
            {
                ok = false;
                errors.Add("Missing destination path.");
            } 
            if(  string.IsNullOrEmpty(fileNameGenerator.pattern))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get { yield return m_VideoSelector.selected; }
        }

        public override void SelfAdjustSettings()
        {
            var input = m_VideoSelector.selected;
            
            if (input == null)
                return;

            var renderTextureSamplerSettings = input as RenderTextureSamplerSettings;
            if (renderTextureSamplerSettings != null)
            {
                var colorSpace = m_OutputFormat == ImageRecorderOutputFormat.EXR ? ColorSpace.Linear : ColorSpace.Gamma;
                renderTextureSamplerSettings.colorSpace = colorSpace;
            }
        }
    }
}
