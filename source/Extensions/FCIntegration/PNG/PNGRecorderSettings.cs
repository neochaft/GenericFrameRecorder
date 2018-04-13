using System;

namespace UTJ.FrameCapturer.Recorders
{
    public class PNGRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcPngConfig m_PngEncoderSettings = fcAPI.fcPngConfig.default_value;

        public PNGRecorderSettings()
        {
            fileNameGenerator.pattern = "image_<0000>.<ext>";
        }
        
        public override string extension
        {
            get { return "png"; }
        }
    }
}
