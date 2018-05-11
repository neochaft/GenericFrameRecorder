using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Recorder
{
    [Serializable]
    abstract class InputSettingsSelector
    {
        [SerializeField] string m_Selected;
        
        readonly Dictionary<string, RecorderInputSetting> m_RecorderInputSettings = new Dictionary<string, RecorderInputSetting>();
        
        public RecorderInputSetting selected
        {
            get
            {
                if (string.IsNullOrEmpty(m_Selected) || !m_RecorderInputSettings.ContainsKey(m_Selected))
                    m_Selected = m_RecorderInputSettings.Keys.First();
                
                return m_RecorderInputSettings[m_Selected];
            }

            set
            {
                foreach (var field in InputSettingFields()) // TODO This is bad
                {
                    var input = (RecorderInputSetting)field.GetValue(this);

                    if (input.GetType() == value.GetType())
                    {
                        field.SetValue(this, value);
                        m_Selected = field.Name;
                        break;
                    }
                }
            }
        }

        public IEnumerable<FieldInfo> InputSettingFields()
        {
            return GetInputFields(GetType()).Where(f => typeof(RecorderInputSetting).IsAssignableFrom(f.FieldType));
        }

        public static IEnumerable<FieldInfo> GetInputFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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