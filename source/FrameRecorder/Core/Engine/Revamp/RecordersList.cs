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
        
        public Recorder2Settings Add(Type type)
        {
            var s = (Recorder2Settings)CreateInstance(type); // TODO Make sure Type is actually a derivate of the recorders base
            s.displayName = type.Name;
            m_Recorders.Add(s);
            
            return s;
        }
    }
}