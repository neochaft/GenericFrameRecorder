#if UNITY_2017_3_OR_NEWER

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

    //[ExecuteInEditMode]
    public class MediaRecorderSettings : UnityEngine.Recorder.RecorderSettings // TODO Rename
    {
        public MediaRecorderOutputFormat m_OutputFormat = MediaRecorderOutputFormat.MP4;
#if UNITY_2018_1_OR_NEWER
        public UnityEditor.VideoBitrateMode m_VideoBitRateMode = UnityEditor.VideoBitrateMode.High;
#endif
        public bool m_AppendSuffix = false;

        MediaRecorderSettings()
        {
            baseFileName.pattern = "movie.<ext>";
        }

        public override List<RecorderInputSetting> GetDefaultInputSettings()
        {
            return new List<RecorderInputSetting>()
            {
                NewInputSettingsObj<CBRenderTextureInputSettings>(),
                NewInputSettingsObj<AudioInputSettings>()
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

        public override RecorderInputSetting NewInputSettingsObj(Type type)
        {
            var obj = base.NewInputSettingsObj(type);

            var imageInput = obj as ImageInputSettings;
            if (imageInput != null)
            {
                imageInput.forceEvenSize = true;
            }
            
            var cbRenderTexture = obj as CBRenderTextureInputSettings;
            if (cbRenderTexture != null)
            {
                cbRenderTexture.flipFinalOutput = Application.platform == RuntimePlatform.OSXEditor;
            }

            return obj ;
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
                },
                
                new List<Type>
                {
                    typeof(AudioInputSettings)
                }
            };
        }

        public override void SelfAdjustSettings()
        {
            if (inputsSettings.Count == 0 )
                return;

            var iis = inputsSettings[0] as ImageInputSettings;
            if (iis != null)
            {
                var maxRes = m_OutputFormat == MediaRecorderOutputFormat.MP4 ? EImageDimension.x2160p_4K : EImageDimension.x4320p_8K;
                iis.maxSupportedSize = maxRes;
            }
        }
       
    }
}

#endif