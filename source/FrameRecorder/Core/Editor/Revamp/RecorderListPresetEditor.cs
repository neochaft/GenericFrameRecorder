using System;
using System.Collections.Generic;

namespace UnityEditor.Recorder
{  
    [CustomEditor(typeof(RecorderListPreset))]
    public class RecorderListPresetEditor : Editor
    {
        Editor m_Editor;
        List<Editor> m_RecordersEditors;
        
        public override void OnInspectorGUI()
        {
            var preset = (RecorderListPreset) target;
 
            if (m_Editor == null)
            {
                m_Editor = CreateEditor(preset.model);

                m_RecordersEditors  = new List<Editor>();
                
                foreach (var p in preset.recorderPresets)
                    m_RecordersEditors.Add(CreateEditor(p));
            }

            m_Editor.OnInspectorGUI();

            foreach (var editor in m_RecordersEditors)
            {
                EditorGUILayout.Separator();
                editor.OnInspectorGUI();
            }
        }

        void OnDestroy()
        {
            DestroyImmediate(m_Editor);
            
            foreach (var editor in m_RecordersEditors)
                DestroyImmediate(editor);
        }
    }
}