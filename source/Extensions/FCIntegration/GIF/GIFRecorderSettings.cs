using System;
using UnityEngine.Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    public class GIFRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcGifConfig m_GifEncoderSettings = fcAPI.fcGifConfig.default_value;

        public GIFRecorderSettings()
        {
            fileNameGenerator.pattern = "image_" + FileNameGenerator.GetTagPattern(ETags.Frame);
        }
        
        public override string extension
        {
            get { return "gif"; }
        }
    }
}
