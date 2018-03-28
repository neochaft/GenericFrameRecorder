using System;
using System.Collections.Generic;
using UnityEditor.Recorder;
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

        public void Remove(RecorderSettings s)
        {
            m_Recorders.Remove(s);
        }
        
        public void Replace(RecorderSettings s, RecorderSettings newSettings)
        {
            var i = m_Recorders.IndexOf(s);
            m_Recorders[i] = newSettings;
        }
    }
}