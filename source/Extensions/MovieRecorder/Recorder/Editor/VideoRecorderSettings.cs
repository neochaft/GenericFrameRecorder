using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Recorder
{
    public enum MediaRecorderOutputFormat
    {
        MP4,
        WEBM
    }

    [Serializable]
    public class VideoSelector : InputSettingsSelector
    {      
        [SerializeField] CBRenderTextureInputSettings m_CbRenderTextureInputSettings = new CBRenderTextureInputSettings();
        [SerializeField] ScreenCaptureInputSettings m_ScreenCaptureInputSettings = new ScreenCaptureInputSettings();
        [SerializeField] Camera360InputSettings m_Camera360InputSettings = new Camera360InputSettings();
        [SerializeField] RenderTextureInputSettings m_RenderTextureInputSettings = new RenderTextureInputSettings();
        [SerializeField] RenderTextureSamplerSettings m_RenderTextureSamplerSettings = new RenderTextureSamplerSettings();

        public VideoSelector()
        {                              
            m_CbRenderTextureInputSettings.forceEvenSize = true;
            m_CbRenderTextureInputSettings.flipFinalOutput = Application.platform == RuntimePlatform.OSXEditor;
            
            m_ScreenCaptureInputSettings.forceEvenSize = true;
                
            m_Camera360InputSettings.forceEvenSize = true;
                
            m_RenderTextureInputSettings.forceEvenSize = true;
                
            m_RenderTextureSamplerSettings.forceEvenSize = true;
        }
    }

    public class VideoRecorderSettings : RecorderSettings
    {
        public MediaRecorderOutputFormat m_OutputFormat;
        public VideoBitrateMode m_VideoBitRateMode = VideoBitrateMode.High;
        public bool m_CaptureAlpha;

        [SerializeField] VideoSelector m_VideoSelector = new VideoSelector();
        [SerializeField] AudioInputSettings m_AudioInputSettings;
        
        public VideoRecorderSettings()
        {
            fileNameGenerator.pattern = "movie.<ext>";
            m_OutputFormat = MediaRecorderOutputFormat.MP4;
            m_VideoBitRateMode = VideoBitrateMode.High;
            m_CaptureAlpha = false;
            ((ImageInputSettings)m_VideoSelector.selected).maxSupportedSize = EImageDimension.x2160p_4K;
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
            get
            {
                yield return m_VideoSelector.selected;
                yield return m_AudioInputSettings;
            }
        }

        public override string extension
        {
            get { return m_OutputFormat.ToString().ToLower(); }
        }

        public override Vector2 resolution
        {
            get
            {
                var inputSettings = (ImageInputSettings)m_VideoSelector.selected; // TODO Refactor commun code
                
                var h = (int)inputSettings.outputSize;
                var w = (int)(h * AspectRatioHelper.GetRealAR(inputSettings.aspectRatio));

                return new Vector2(w, h);
            }
        }

        public override void SelfAdjustSettings()
        {
            var selectedInput = m_VideoSelector.selected;
            if (selectedInput == null)
                return;

            ((ImageInputSettings)m_VideoSelector.selected).maxSupportedSize = m_OutputFormat == MediaRecorderOutputFormat.MP4 ? EImageDimension.x2160p_4K : EImageDimension.x4320p_8K;

            var cbis = selectedInput as CBRenderTextureInputSettings;
            if (cbis != null)
            {
                cbis.allowTransparency = m_OutputFormat == MediaRecorderOutputFormat.WEBM && m_CaptureAlpha;
            }
        }
       
    }
}