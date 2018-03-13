﻿using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    /// <summary>
    /// This is just a helper class that should disappear once we have a proper way of saving the recorder window settings...
    /// </summary>
    public class RecorderWindowSettings : ScriptableObject
    {
        public RecorderSettings m_Settings;
        
        public RecorderWindowSettings Clone()
        {
            return Instantiate(this);
            //var rws = Instantiate(this);
            //rws.m_Settings = Instantiate(m_Settings);
            //return rws;
        }
    }
}