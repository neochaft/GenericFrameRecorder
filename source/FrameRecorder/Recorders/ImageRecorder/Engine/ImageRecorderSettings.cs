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
        [SerializeField] public CBRenderTextureInputSettings cbRenderTextureInputSettings = new CBRenderTextureInputSettings();
        [SerializeField] public ScreenCaptureInputSettings screenCaptureInputSettings = new ScreenCaptureInputSettings();
        [SerializeField] public Camera360InputSettings camera360InputSettings = new Camera360InputSettings();
        [SerializeField] public RenderTextureInputSettings renderTextureInputSettings = new RenderTextureInputSettings();
        [SerializeField] public RenderTextureSamplerSettings renderTextureSamplerSettings = new RenderTextureSamplerSettings();
    }
    
    public class ImageRecorderSettings : RecorderSettings
    {
        public ImageRecorderOutputFormat outputFormat = ImageRecorderOutputFormat.JPEG;

        [SerializeField] VideoSelector m_VideoSelector = new VideoSelector();

        public ImageRecorderSettings()
        {
            fileNameGenerator.pattern = "image_" + FileNameGenerator.DefaultWildcard.Frame;
        }
        
        public override string extension
        {
            get
            {
                switch (outputFormat)
                {
                    case ImageRecorderOutputFormat.PNG:                        
                        return "png";
                    case ImageRecorderOutputFormat.JPEG:                        
                        return "jpg";
                    case ImageRecorderOutputFormat.EXR:                        
                        return "exr";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public override Vector2 resolution
        {
            get
            {
                var inputSettings = (ImageInputSettings)m_VideoSelector.selected;
                
                var h = (int)inputSettings.outputSize;
                var w = (int)(h * AspectRatioHelper.GetRealAR(inputSettings.aspectRatio));

                return new Vector2(w, h);
            }
        }
        
        public override bool ValidityCheck(List<string> errors)
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
                var colorSpace = outputFormat == ImageRecorderOutputFormat.EXR ? ColorSpace.Linear : ColorSpace.Gamma;
                renderTextureSamplerSettings.colorSpace = colorSpace;
            }
        }
    }
}
