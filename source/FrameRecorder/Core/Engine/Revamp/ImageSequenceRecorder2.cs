namespace UnityEngine.Recorder
{
    public class ImageSequenceRecorder2 : Recorder2Settings //Reco
    {
        public int imageSeqStuff;
        
        public enum Format
        {
            PNG,
            JPG,
            GIF,
            EXR
        }

        public Format format;
        
    }
}