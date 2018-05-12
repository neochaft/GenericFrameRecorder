using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    public enum VideoRecorderOutputFormat
    {
        MP4,
        WEBM
    }

    [RecorderSettings(typeof(MovieRecorder), "Movie", "movie_recorder")]
    public class MovieRecorderSettings : RecorderSettings
    {
        public VideoRecorderOutputFormat outputFormat = VideoRecorderOutputFormat.MP4;
        public VideoBitrateMode videoBitRateMode = VideoBitrateMode.High;
        public bool captureAlpha;

        [Serializable]
        class VideoInputSelector : InputSettingsSelector
        {      
            [SerializeField] CBRenderTextureInputSettings m_CbRenderTextureInputSettings = new CBRenderTextureInputSettings();
            [SerializeField] GameViewInputSettings m_GameViewInputSettings = new GameViewInputSettings();
            [SerializeField] Camera360InputSettings m_Camera360InputSettings = new Camera360InputSettings();
            [SerializeField] RenderTextureInputSettings m_RenderTextureInputSettings = new RenderTextureInputSettings();
            [SerializeField] RenderTextureSamplerSettings m_RenderTextureSamplerSettings = new RenderTextureSamplerSettings();

            public VideoInputSelector()
            {                              
                m_CbRenderTextureInputSettings.forceEvenSize = true;
                m_CbRenderTextureInputSettings.flipFinalOutput = Application.platform == RuntimePlatform.OSXEditor;
            
                m_GameViewInputSettings.forceEvenSize = true;
                
                m_Camera360InputSettings.forceEvenSize = true;
                
                m_RenderTextureInputSettings.forceEvenSize = true;
                
                m_RenderTextureSamplerSettings.forceEvenSize = true;
            }
        }
        
        [SerializeField] VideoInputSelector m_VideoInputSelector = new VideoInputSelector();
        [SerializeField] AudioInputSettings m_AudioInputSettings = new AudioInputSettings();
        
        public MovieRecorderSettings()
        {
            fileNameGenerator.fileName = "movie";
            ((ImageInputSettings)m_VideoInputSelector.selected).maxSupportedSize = ImageResolution.x2160p_4K;
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

        public RecorderInputSetting videoInputSettings
        {
            get { return m_VideoInputSelector.selected; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value is CBRenderTextureInputSettings ||
                    value is GameViewInputSettings ||
                    value is Camera360InputSettings ||
                    value is RenderTextureInputSettings ||
                    value is RenderTextureSamplerSettings)
                {
                    m_VideoInputSelector.selected = value;
                }
                else
                {
                    throw new ArgumentException("Video input type not support: '" + value.GetType() + "'");
                }
            }
        }

        public AudioInputSettings audioInputSettings
        {
            get { return m_AudioInputSettings; }
        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get
            {
                yield return m_VideoInputSelector.selected;
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
                var inputSettings = (ImageInputSettings)m_VideoInputSelector.selected; // TODO Refactor commun code
                
                var h = (int)inputSettings.outputResolution;
                var w = (int)(h * AspectRatioHelper.GetRealAspect(inputSettings.aspectRatio));

                return new Vector2(w, h);
            }
        }

        public override void SelfAdjustSettings()
        {
            var selectedInput = m_VideoInputSelector.selected;
            if (selectedInput == null)
                return;

            ((ImageInputSettings)m_VideoInputSelector.selected).maxSupportedSize = outputFormat == VideoRecorderOutputFormat.MP4 ? ImageResolution.x2160p_4K : ImageResolution.x4320p_8K;

            var cbis = selectedInput as CBRenderTextureInputSettings;
            if (cbis != null)
            {
                cbis.allowTransparency = outputFormat == VideoRecorderOutputFormat.WEBM && captureAlpha;
            }
        }
       
    }
}