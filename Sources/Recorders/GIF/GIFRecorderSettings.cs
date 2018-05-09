using Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    [RecorderSettings(typeof(GIFRecorder), "GIF" )]
    public class GIFRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcGifConfig m_GifEncoderSettings = fcAPI.fcGifConfig.default_value;

        public GIFRecorderSettings()
        {
            fileNameGenerator.fileName = "image_" + FileNameGenerator.DefaultWildcard.Frame;
        }
        
        public override string extension
        {
            get { return "gif"; }
        }
    }
}
