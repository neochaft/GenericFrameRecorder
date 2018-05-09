using UTJ.FrameCapturer;
using UTJ.FrameCapturer.Recorders;

namespace Recorder
{
    [RecorderSettings(typeof(GIFRecorder), "GIF Animation", "image_recorder")]
    public class GIFRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcGifConfig m_GifEncoderSettings = fcAPI.fcGifConfig.default_value;

        public GIFRecorderSettings()
        {
            fileNameGenerator.fileName = "gif_animation_" + FileNameGenerator.DefaultWildcard.Take;
        }
        
        public override string extension
        {
            get { return "gif"; }
        }
    }
}
