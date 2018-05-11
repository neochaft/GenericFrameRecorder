using UnityEngine;

namespace Recorder.FrameCapturer
{
    [RecorderSettings(typeof(MP4Recorder), "Legacy/MP4" )]
    public class MP4RecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcMP4Config m_MP4EncoderSettings = fcAPI.fcMP4Config.default_value;
        public bool m_AutoSelectBR;

        public MP4RecorderSettings()
        {
            fileNameGenerator.fileName = "movie";
            m_AutoSelectBR = true;
            m_VideoInputSelector.SetMaxResolution(ImageResolution.x2160p_4K);
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;
            }
        }
        
        public override string extension
        {
            get { return "mp4"; }
        }
    }
}
