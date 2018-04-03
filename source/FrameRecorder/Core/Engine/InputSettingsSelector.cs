using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    [Serializable]
    public class InputSettingsSelector
    {
        [SerializeField] string m_Selected;
        
        readonly Dictionary<string, RecorderInputSetting> m_RecorderInputSettings = new Dictionary<string, RecorderInputSetting>();
        
        public RecorderInputSetting selected
        {
            get
            {
                if (string.IsNullOrEmpty(m_Selected))
                    m_Selected = m_RecorderInputSettings.Keys.First();
                
                return m_RecorderInputSettings[m_Selected];
            }
        }

        public IEnumerable<FieldInfo> InputSettingFields()
        {
            return GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => typeof(RecorderInputSetting).IsAssignableFrom(f.FieldType));
        }

        protected InputSettingsSelector()
        {
            foreach (var field in InputSettingFields())
            {
                var input = (RecorderInputSetting)field.GetValue(this);
                m_RecorderInputSettings.Add(field.Name, input);
            }
        }
    }
}