using System;
using UnityEngine;
using UnityEngine.Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    public class EXRRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcExrConfig m_ExrEncoderSettings = fcAPI.fcExrConfig.default_value;

        public EXRRecorderSettings()
        {
            
        }

        public override string extension
        {
            get { return "exr"; }
        }
    }
}
