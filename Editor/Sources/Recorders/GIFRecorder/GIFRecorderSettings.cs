using Recorder.FrameCapturer;

namespace Recorder
{
    [RecorderSettings(typeof(GIFRecorder), "GIF Animation", "image_recorder")]
    class GIFRecorderSettings : BaseFCRecorderSettings
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
