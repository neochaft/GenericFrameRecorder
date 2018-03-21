using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class InputSelector
    {
        readonly RecorderSettings m_RecorderSettings;

        struct InputGroup
        {
            public string[] captions;
            public Type[] types;
        }

        readonly SortedDictionary<int, InputGroup> m_Groups;

        public InputSelector(RecorderSettings recorderSettings)
        {
            m_Groups = new SortedDictionary<int, InputGroup>();
            m_RecorderSettings = recorderSettings;

            AddGroups(recorderSettings.GetInputGroups());
        }

        void AddGroups(InputGroups typeGroups)
        {
            foreach (var group in typeGroups)
            {
                m_Groups.Add(m_Groups.Count,
                    new InputGroup
                    {
                        captions = group.Select(GetTypeDisplayName).ToArray(),
                        types = group.ToArray()
                    });
            }
        }

        static string GetTypeDisplayName(Type type)
        {
            var displayNameAttribute = type.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;

            return displayNameAttribute != null
                ? displayNameAttribute.DisplayName
                : ObjectNames.NicifyVariableName(type.Name);
        }

        public bool OnInputGui(int groupIndex, ref RecorderInputSetting input)
        {
            if (!m_Groups.ContainsKey(groupIndex))
                return false;
            
            if (m_Groups[groupIndex].types.Length < 2)
                return false;

            int index = 0;
            for (int i = 0; i < m_Groups[groupIndex].types.Length; i++)
            {
                if (m_Groups[groupIndex].types[i] == input.GetType())
                {
                    index = i;
                    break;
                }
            }
            
            var newIndex = EditorGUILayout.Popup("Capture", index, m_Groups[groupIndex].captions);

            if (index != newIndex)
            {
                input = m_RecorderSettings.NewInputSettingsObj(m_Groups[groupIndex].types[newIndex]);
                return true;
            }

            return false;
        }
    }

}