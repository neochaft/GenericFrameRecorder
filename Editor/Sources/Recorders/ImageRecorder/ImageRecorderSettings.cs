using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{   
    public enum ImageRecorderOutputFormat
    {
        PNG,
        JPEG,
        EXR
    }
    
    [Serializable]
    class VideoSelector : InputSettingsSelector
    {      
        [SerializeField] public CBRenderTextureInputSettings cbRenderTextureInputSettings = new CBRenderTextureInputSettings();
        [SerializeField] public GameViewInputSettings gameViewInputSettings = new GameViewInputSettings();
        [SerializeField] public Camera360InputSettings camera360InputSettings = new Camera360InputSettings();
        [SerializeField] public RenderTextureInputSettings renderTextureInputSettings = new RenderTextureInputSettings();
        [SerializeField] public RenderTextureSamplerSettings renderTextureSamplerSettings = new RenderTextureSamplerSettings();
    }
    
    [RecorderSettings(typeof(ImageRecorder), "Image Sequence", "image_recorder")]
    public class ImageRecorderSettings : RecorderSettings
    {
        public ImageRecorderOutputFormat outputFormat = ImageRecorderOutputFormat.JPEG;
        public bool captureAlpha;

        [SerializeField] VideoSelector m_VideoInputSelector = new VideoSelector();

        public ImageRecorderSettings()
        {
            fileNameGenerator.fileName = "image_" + FileNameGenerator.DefaultWildcard.Frame;
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
                var inputSettings = (ImageInputSettings)m_VideoInputSelector.selected;
                
                var h = (int)inputSettings.outputResolution;
                var w = (int)(h * AspectRatioHelper.GetRealAspect(inputSettings.aspectRatio));

                return new Vector2(w, h);
            }
        }
        
        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
 
            if(string.IsNullOrEmpty(fileNameGenerator.fileName))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get { yield return m_VideoInputSelector.selected; }
        }

        public override void SelfAdjustSettings()
        {
            var input = m_VideoInputSelector.selected;
            
            if (input == null)
                return;

            var renderTextureSamplerSettings = input as RenderTextureSamplerSettings;
            if (renderTextureSamplerSettings != null)
            {
                var colorSpace = outputFormat == ImageRecorderOutputFormat.EXR ? ColorSpace.Linear : ColorSpace.Gamma;
                renderTextureSamplerSettings.colorSpace = colorSpace;
            }
            
            var cbis = input as CBRenderTextureInputSettings;
            if (cbis != null)
            {
                cbis.allowTransparency = (outputFormat == ImageRecorderOutputFormat.PNG || outputFormat == ImageRecorderOutputFormat.EXR) && captureAlpha;
            }
        }
    }
}
