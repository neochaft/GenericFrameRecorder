using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Recorder
{
    public class RecordersList : ScriptableObject
    {
        [SerializeField] List<Recorder2Settings> m_Recorders = new List<Recorder2Settings>();
        
        public IEnumerable<Recorder2Settings> recorders
        {
            get { return m_Recorders; }
        }
        
        public void Add(Recorder2Settings s)
        {
            m_Recorders.Add(s);
        }
    }
}