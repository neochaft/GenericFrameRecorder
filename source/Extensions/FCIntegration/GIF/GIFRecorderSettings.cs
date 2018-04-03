using System;

namespace UTJ.FrameCapturer.Recorders
{
    public class GIFRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcGifConfig m_GifEncoderSettings = fcAPI.fcGifConfig.default_value;

        GIFRecorderSettings()
        {
            baseFileName.pattern = "image.<ext>";
        }
    }
}
