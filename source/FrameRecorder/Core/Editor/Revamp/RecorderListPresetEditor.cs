using System.Collections.Generic;
using UnityEditor;

namespace Recorder
{  
    [CustomEditor(typeof(RecorderListPreset))]
    public class RecorderListPresetEditor : Editor
    {
        Editor m_Editor;

        class PresetEditorState
        {
            public bool expanded;
            public Editor presetEditor;
            public SerializedProperty enabledProperty;
            public SerializedProperty displayNameProperty;
        }
        
        readonly List<PresetEditorState> m_RecordersEditors = new List<PresetEditorState>();

        void OnEnable()
        {
            if (target == null)
                return;
            
            var preset = (RecorderListPreset) target;
            
            m_Editor = CreateEditor(preset.model);

            m_RecordersEditors.Clear();

            var recorderPresetInfos = serializedObject.FindProperty("m_RecorderPresetInfos");

            var recorderPresets = preset.recorderPresets;

            for (int i = 0; i < recorderPresetInfos.arraySize; ++i)
            {
                var e = recorderPresetInfos.GetArrayElementAtIndex(i);
  
                var state = new PresetEditorState
                {
                    presetEditor = CreateEditor(recorderPresets[i]),
                    displayNameProperty = e.FindPropertyRelative("m_DisplayName"),
                    enabledProperty = e.FindPropertyRelative("m_Enabled")
                };
                
                m_RecordersEditors.Add(state);
            }
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;
            
            m_Editor.OnInspectorGUI();
            
            EditorGUILayout.Separator();
            
            foreach (var state in m_RecordersEditors)
            {
                if (FoldoutPresetEditorStateHeader(state))
                {
                    EditorGUILayout.Separator();
                    state.presetEditor.OnInspectorGUI();
                }
            }            
        }

        static bool FoldoutPresetEditorStateHeader(PresetEditorState state)
        {
            var r = EditorGUILayout.GetControlRect();
            var rFold = r;
            rFold.width = 0;
            state.expanded = EditorGUI.Foldout(rFold, state.expanded, string.Empty);
            
            var rToggle = r;
            rToggle.x = 20;
            rToggle.width = 20;
            
            EditorGUI.BeginChangeCheck();
            state.enabledProperty.boolValue = EditorGUI.Toggle(rToggle, state.enabledProperty.boolValue);
            if (EditorGUI.EndChangeCheck())
                state.enabledProperty.serializedObject.ApplyModifiedProperties();

            var rName = r;
            rName.xMin += 25;
            
            EditorGUI.BeginChangeCheck();
            state.displayNameProperty.stringValue = EditorGUI.TextField(rName, state.displayNameProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
                state.displayNameProperty.serializedObject.ApplyModifiedProperties();

            return state.expanded;
        }

        void OnDestroy()
        {
            if (m_Editor != null)
            {
                DestroyImmediate(m_Editor);
                m_Editor = null;
            }

            foreach (var state in m_RecordersEditors)
                DestroyImmediate(state.presetEditor);
            
            m_RecordersEditors.Clear();
        }
    }
}