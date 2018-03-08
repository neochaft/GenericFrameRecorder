using System;
using UnityEngine;

namespace UnityEngine.Recorder
{
    public enum FrameResolution
    {
        X240P,
        X480P,
        X720P_HD,
        X1080P_FHD,
        X1440P_QHD,
        X2160P_4K,
        X2880P_5K,
        X4320P_8K,
        Custom
    }

//    [Serializable]
//    public abstract class Reco// : ScriptableObject
//    {
//        //public abstract void OnGUI();
//        public string test;
//    }
    
    //[Serializable]
    public abstract class Recorder2Settings : ScriptableObject //where T : Reco
    {
        public string displayName;
        public FrameResolution resolution;
        
        
        //public T reco;
    }
}