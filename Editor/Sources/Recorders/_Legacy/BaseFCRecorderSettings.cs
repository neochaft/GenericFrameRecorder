using System;
using System.Collections.Generic;
using UnityEngine;
using Recorder;
using Recorder.Input;

namespace FrameCapturer.Recorders
{
    [Serializable]
    public class UTJVideoSelector : InputSettingsSelector
    {
        [SerializeField] CBRenderTextureInputSettings m_CbRenderTextureInputSettings = new CBRenderTextureInputSettings();
        [SerializeField] RenderTextureSamplerSettings m_RenderTextureSamplerSettings = new RenderTextureSamplerSettings();
        [SerializeField] RenderTextureInputSettings m_RenderTextureInputSettings = new RenderTextureInputSettings();
        
        public UTJVideoSelector()
        {
            m_CbRenderTextureInputSettings.flipFinalOutput = true;
            m_RenderTextureSamplerSettings.flipFinalOutput = true;
        }

        public void SetMaxResolution(ImageResolution maxSupportedSize)
        {
            m_CbRenderTextureInputSettings.maxSupportedSize = maxSupportedSize;
            m_RenderTextureSamplerSettings.maxSupportedSize = maxSupportedSize;
            m_RenderTextureInputSettings.maxSupportedSize = maxSupportedSize;
        }
    }
    
    public abstract class BaseFCRecorderSettings : RecorderSettings
    {
        [SerializeField] protected UTJVideoSelector m_VideoInputSelector = new UTJVideoSelector();

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if(  string.IsNullOrEmpty(fileNameGenerator.fileName))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }
        
        public override Vector2 resolution
        {
            get
            {
                var inputSettings = (ImageInputSettings)m_VideoInputSelector.selected; // TODO Refactor commun code
                
                var h = (int)inputSettings.outputResolution;
                var w = (int)(h * AspectRatioHelper.GetRealAspect(inputSettings.aspectRatio));

                return new Vector2(w, h);
            }
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

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get { yield return m_VideoInputSelector.selected; }
        }
    }
}
