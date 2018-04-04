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
            m_ScreenCaptureInputSettings.forceEvenSize = true;
                
            m_CbRenderTextureInputSettings.forceEvenSize = true;
            m_CbRenderTextureInputSettings.flipFinalOutput = Application.platform == RuntimePlatform.OSXEditor;
                
            m_Camera360InputSettings.forceEvenSize = true;
                
            m_RenderTextureInputSettings.forceEvenSize = true;
                
            m_RenderTextureSamplerSettings.forceEvenSize = true;
        }
    }

    public class MediaRecorderSettings : RecorderSettings // TODO Rename
    {
        public MediaRecorderOutputFormat m_OutputFormat = MediaRecorderOutputFormat.MP4;
        public VideoBitrateMode m_VideoBitRateMode = VideoBitrateMode.High;
        public bool m_AppendSuffix = false;

//        [SerializeField] ScreenCaptureInputSettings m_ScreenCaptureInputSettings = new ScreenCaptureInputSettings();
//        [SerializeField] CBRenderTextureInputSettings m_CbRenderTextureInputSettings = new CBRenderTextureInputSettings();
//        [SerializeField] Camera360InputSettings m_Camera360InputSettings = new Camera360InputSettings();
//        [SerializeField] RenderTextureInputSettings m_RenderTextureInputSettings = new RenderTextureInputSettings();
//        [SerializeField] RenderTextureSamplerSettings m_RenderTextureSamplerSettings = new RenderTextureSamplerSettings();
//
        [SerializeField] VideoSelector m_VideoSelector = new VideoSelector();
        [SerializeField] AudioInputSettings m_AudioInputSettings;
        //InputSettingsList m_InputsSettings;


        public MediaRecorderSettings()
        {
            baseFileName.pattern = "movie.<ext>";
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

//        public override RecorderInputSetting NewInputSettingsObj(Type type)
//        {
//            var obj = base.NewInputSettingsObj(type);
//
//            var imageInput = obj as ImageInputSettings;
//            if (imageInput != null)
//            {
//                imageInput.forceEvenSize = true;
//            }
//            
//            var cbRenderTexture = obj as CBRenderTextureInputSettings;
//            if (cbRenderTexture != null)
//            {
//                cbRenderTexture.flipFinalOutput = Application.platform == RuntimePlatform.OSXEditor;
//            }
//
//            return obj ;
//        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get
            {
                yield return m_VideoSelector.selected;
                yield return m_AudioInputSettings;
            }
        }

        public override void SelfAdjustSettings()
        {
            var selectedInput = m_VideoSelector.selected;
            if (selectedInput == null)
                return;

            var iis = selectedInput as ImageInputSettings;
            if (iis != null)
            {
                var maxRes = m_OutputFormat == MediaRecorderOutputFormat.MP4 ? EImageDimension.x2160p_4K : EImageDimension.x4320p_8K;
                iis.maxSupportedSize = maxRes;
            }
        }
       
    }
}