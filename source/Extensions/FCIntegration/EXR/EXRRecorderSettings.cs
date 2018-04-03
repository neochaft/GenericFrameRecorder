using System;

namespace UTJ.FrameCapturer.Recorders
{
    public class EXRRecorderSettings : BaseFCRecorderSettings
    {

        public fcAPI.fcExrConfig m_ExrEncoderSettings = fcAPI.fcExrConfig.default_value;
        

        EXRRecorderSettings()
        {
            baseFileName.pattern = "image_<0000>.<ext>";
        }
    }
}
