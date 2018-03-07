using System;
using UnityEngine;

namespace UnityEditor.Recorder
{
    //[Serializable]
    public abstract class Recorder2Settings : ScriptableObject
    {
        public string displayName;
        
        
        
    }
    
    public class MovieRecorder2 : Recorder2Settings
    {
        public int movieStuff;
        
        
        
    }
    
    public class ImageSequenceRecorder2 : Recorder2Settings
    {
        public int imageSeqStuff;
        
        
        
    }
    
    public class AnimationRecorder2 : Recorder2Settings
    {
        public int animationStuff;
        
        
        
    }
}