using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Recorder
{
    public class RecordersList : ScriptableObject
    {
        [SerializeField] List<Recorder2Settings> m_Recorders = new List<Recorder2Settings>();
        
        public IEnumerable<Recorder2Settings> recorders
        {
            get { return m_Recorders; }
        }
        
        public Recorder2Settings Add(string type)
        {
            var s = new Recorder2Settings() {name = type};
            m_Recorders.Add(s);
            
            return s;
        }
    }

    [Serializable]
    public class Recorder2Settings
    {
        public string name;
    }
}