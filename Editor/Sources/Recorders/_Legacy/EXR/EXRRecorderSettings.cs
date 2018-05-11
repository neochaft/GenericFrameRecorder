using Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    [RecorderSettings(typeof(EXRRecorder), "Legacy/OpenEXR")]
    public class EXRRecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcExrConfig m_ExrEncoderSettings = fcAPI.fcExrConfig.default_value;

        public override string extension
        {
            get { return "exr"; }
        }
    }
}
