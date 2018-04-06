namespace UnityEngine.Recorder
{
    public class MovieRecorder2 : Recorder2Settings //Reco
    {
        public int movieStuff;
        public FrameRateType rate;


        public enum Format
        {
            MP4,
            WEBM
        }

        public Format format;

    }
    
}