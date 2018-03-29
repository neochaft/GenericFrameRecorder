using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UTJ.FrameCapturer.Recorders
{
    public class WEBMRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcWebMConfig m_WebmEncoderSettings = fcAPI.fcWebMConfig.default_value;
        public bool m_AutoSelectBR;

        WEBMRecorderSettings()
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
                (obj as CBRenderTextureInputSettings).forceEvenSize = true;

            return obj ;
        }

    }
}
