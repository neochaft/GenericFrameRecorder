using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UTJ.FrameCapturer.Recorders
{
    public class PNGRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcPngConfig m_PngEncoderSettings = fcAPI.fcPngConfig.default_value;

        PNGRecorderSettings()
        {
            baseFileName.pattern = "image_<0000>.<ext>";
        }

        public override List<RecorderInputSetting> GetDefaultInputSettings()
        {
            return new List<RecorderInputSetting>() { NewInputSettingsObj<CBRenderTextureInputSettings>() };
        }
    }
}
