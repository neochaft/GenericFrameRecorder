using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Recorder
{
    public class RecordersList : ScriptableObject
    {
        [SerializeField] List<RecorderSettings> m_Recorders = new List<RecorderSettings>();
        
        public IEnumerable<RecorderSettings> recorders
        {
            get { return m_Recorders; }
        }
     
        public void Add(RecorderSettings s)
        {
            m_Recorders.Add(s);
        }
    }
}