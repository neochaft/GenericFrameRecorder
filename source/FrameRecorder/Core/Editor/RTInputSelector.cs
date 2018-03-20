using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class RTInputSelector
    {
        readonly RecorderSettings m_RecorderSettings;

        struct InputGroup
        {
            public string title;
            public string[] captions;
            public Type[] types;
        }

        readonly SortedDictionary<int, InputGroup> m_Groups;

        public RTInputSelector( RecorderSettings recorderSettings  )
        {
            m_Groups = new SortedDictionary<int, InputGroup>();
            m_RecorderSettings = recorderSettings;

            AddGroups( recorderSettings.GetInputGroups() );
        }

        void AddGroups(IList<InputGroupFilter> groups)
        {
            for(int i = 0; i < groups.Count; i++)
            {
                m_Groups.Add(m_Groups.Count,
                    new InputGroup()
                    {
                        title = groups[i].title,
                        captions = groups[i].typesFilter.Select(x => x.title).ToArray(),
                        types = groups[i].typesFilter.Select(x => x.type).ToArray(),
                    });
            }
        }

        public bool OnInputGui( int groupIndex, ref RecorderInputSetting input)
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
                input = m_RecorderSettings.NewInputSettingsObj( m_Groups[groupIndex].types[newIndex], m_Groups[groupIndex].title );
                return true;
            }

            return false;
        }
    }

}