using System;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UTJ.FrameCapturer.Recorders
{
    public class MP4RecorderSettings : BaseFCRecorderSettings
    {
        public fcAPI.fcMP4Config m_MP4EncoderSettings = fcAPI.fcMP4Config.default_value;
        public bool m_AutoSelectBR;

        public MP4RecorderSettings()
        {
            fileNameGenerator.pattern = "movie";
            m_AutoSelectBR = true;
            m_VideoSelector.SetMaxResolution(ImageDimension.x2160p_4K);
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
