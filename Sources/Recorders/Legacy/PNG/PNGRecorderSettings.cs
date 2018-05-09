using Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    [RecorderSettings(typeof(PNGRecorder), "Legacy/PNG" )]
    public class PNGRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcPngConfig m_PngEncoderSettings = fcAPI.fcPngConfig.default_value;

        public PNGRecorderSettings()
        {
            fileNameGenerator.fileName = "image_" + FileNameGenerator.DefaultWildcard.Frame;
        }
        
        public override string extension
        {
            get { return "png"; }
        }
    }
}
