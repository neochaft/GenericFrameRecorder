using System;

namespace UnityEditor.Recorder
{  
    [CustomEditor(typeof(RecorderListPreset))]
    public class RecorderListPresetEditor : Editor
    {
        Editor m_Editor;
        
        public override void OnInspectorGUI()
        {
            var preset = (RecorderListPreset) target;

            if (preset.model == null)
            {
                EditorGUILayout.LabelField("Preset corrupted.");
                return;
            }

            EditorGUILayout.HelpBox("Recorder Preset", MessageType.None);            
            EditorGUILayout.Separator();
            
            if (m_Editor == null)
                m_Editor = CreateEditor(preset.model);
            
            m_Editor.OnInspectorGUI();
        }
    }
}