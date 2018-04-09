using System;
using System.Collections.Generic;

namespace UnityEditor.Recorder
{  
    [CustomEditor(typeof(RecorderListPreset))]
    public class RecorderListPresetEditor : Editor
    {
        Editor m_Editor;
        readonly List<Editor> m_RecordersEditors = new List<Editor>();

        void OnEnable()
        {
            var preset = (RecorderListPreset) target;
            
            m_Editor = CreateEditor(preset.model);

            m_RecordersEditors.Clear();
            
            foreach (var p in preset.recorderPresets)
                m_RecordersEditors.Add(CreateEditor(p));
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;
            
            m_Editor.OnInspectorGUI();

            foreach (var editor in m_RecordersEditors)
            {
                EditorGUILayout.Separator();
                editor.OnInspectorGUI();
            }
        }

        void OnDestroy()
        {
            if (m_Editor != null)
            {
                DestroyImmediate(m_Editor);
                m_Editor = null;
            }

            foreach (var editor in m_RecordersEditors)
                DestroyImmediate(editor);
            
            m_RecordersEditors.Clear();
        }
    }
}