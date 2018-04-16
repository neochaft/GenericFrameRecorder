using System;
using UnityEngine.Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    public class PNGRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcPngConfig m_PngEncoderSettings = fcAPI.fcPngConfig.default_value;

        public PNGRecorderSettings()
        {
            fileNameGenerator.pattern = "image_" + FileNameGenerator.GetTagPattern(ETags.Frame);
        }
        
        public override string extension
        {
            get { return "png"; }
        }
    }
}
