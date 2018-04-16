using System;

namespace UTJ.FrameCapturer.Recorders
{
    public class WEBMRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcWebMConfig m_WebmEncoderSettings = fcAPI.fcWebMConfig.default_value;
        public bool m_AutoSelectBR;

        public WEBMRecorderSettings()
        {
            fileNameGenerator.pattern = "movie";
            m_AutoSelectBR = true;
        }

        public override string extension
        {
            get { return "webm"; }
        }
    }
}
