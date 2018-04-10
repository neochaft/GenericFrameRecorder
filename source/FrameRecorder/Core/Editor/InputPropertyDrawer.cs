using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public abstract class InputPropertyDrawer<T> : TargetedPropertyDrawer<T> where T : class
    {
        protected List<string> m_SettingsErrors = new List<string>();

        public delegate EFieldDisplayState IsFieldAvailableDelegate(SerializedProperty property);

        public IsFieldAvailableDelegate isFieldAvailableForHost { get; set; }

        protected virtual EFieldDisplayState IsFieldAvailable(SerializedProperty property)
        {
            return EFieldDisplayState.Enabled;
        }

        public virtual void OnValidateSettingsGUI()
        {
//            m_SettingsErrors.Clear();
//            if (!(target as RecorderInputSetting).ValidityCheck(m_SettingsErrors))
//            {
//                for (int i = 0; i < m_SettingsErrors.Count; i++)
//                {
//                    EditorGUILayout.HelpBox(m_SettingsErrors[i], MessageType.Warning);
//                }
//            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }

        public virtual void CaptureOptionsGUI()
        {
            
        }
    }
}
