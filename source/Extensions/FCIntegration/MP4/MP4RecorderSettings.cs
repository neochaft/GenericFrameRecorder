using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UTJ.FrameCapturer.Recorders
{
    public class MP4RecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcMP4Config m_MP4EncoderSettings = fcAPI.fcMP4Config.default_value;
        public bool m_AutoSelectBR;

        MP4RecorderSettings()
        {
            baseFileName.pattern = "movie.<ext>";
            m_AutoSelectBR = true;
        }

        public override List<RecorderInputSetting> GetDefaultInputSettings()
        {
            return new List<RecorderInputSetting>()
            {
                NewInputSettingsObj<CBRenderTextureInputSettings>() 
            };
        }

        public override RecorderInputSetting NewInputSettingsObj(Type type)
        {
            var obj = base.NewInputSettingsObj(type);
            if (type == typeof(CBRenderTextureInputSettings))
            {
                var settings = (CBRenderTextureInputSettings)obj;
                settings.forceEvenSize = true;
                settings.flipFinalOutput = true;
            }

            return obj ;
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;
            }
        }

        public override void SelfAdjustSettings()
        {
            if (inputsSettings.Count == 0 )
                return;

            var iis = inputsSettings[0] as ImageInputSettings;
            if (iis != null)
            {
                iis.maxSupportedSize = EImageDimension.x2160p_4K;
            }
        }

    }
}
