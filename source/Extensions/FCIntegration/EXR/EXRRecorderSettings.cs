using System;
using UnityEngine;

namespace UTJ.FrameCapturer.Recorders
{
    public class EXRRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcExrConfig m_ExrEncoderSettings = fcAPI.fcExrConfig.default_value;

        public EXRRecorderSettings()
        {
            fileNameGenerator.pattern = "image_<0000>.<ext>";
        }

        public override string extension
        {
            get { return "exr"; }
        }
    }
}
