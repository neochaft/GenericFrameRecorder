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
        public MediaRecorderOutputFormat outputFormat = MediaRecorderOutputFormat.MP4;
        public VideoBitrateMode videoBitRateMode = VideoBitrateMode.High;
        public bool captureAlpha;

        [SerializeField] VideoSelector m_VideoSelector = new VideoSelector();
        [SerializeField] AudioInputSettings m_AudioInputSettings;
        
        public VideoRecorderSettings()
        {
            fileNameGenerator.fileName = "movie";
            ((ImageInputSettings)m_VideoSelector.selected).maxSupportedSize = ImageDimension.x2160p_4K;
        }

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if( string.IsNullOrEmpty(fileNameGenerator.fileName))
            {
                errors.Add("Missing file name");
                ok = false;
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
            get { return outputFormat.ToString().ToLower(); }
        }

        public override Vector2 resolution
        {
            get
            {
                var inputSettings = (ImageInputSettings)m_VideoSelector.selected; // TODO Refactor commun code
                
                var h = (int)inputSettings.outputSize;
                var w = (int)(h * AspectRatioHelper.GetRealAspect(inputSettings.aspectRatio));

                return new Vector2(w, h);
            }
        }

        public override void SelfAdjustSettings()
        {
            var selectedInput = m_VideoSelector.selected;
            if (selectedInput == null)
                return;

            ((ImageInputSettings)m_VideoSelector.selected).maxSupportedSize = outputFormat == MediaRecorderOutputFormat.MP4 ? ImageDimension.x2160p_4K : ImageDimension.x4320p_8K;

            var cbis = selectedInput as CBRenderTextureInputSettings;
            if (cbis != null)
            {
                cbis.allowTransparency = outputFormat == MediaRecorderOutputFormat.WEBM && captureAlpha;
            }
        }
       
    }
}