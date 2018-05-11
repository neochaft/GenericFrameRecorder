using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(RecorderSettingsPrefs))]
    class RecorderSettingsPrefsEditor : Editor
    {
        SerializedProperty m_RecordModeProperty;
        
        SerializedProperty m_PlaybackProperty;
        SerializedProperty m_FrameRateTypeProperty;
        SerializedProperty m_CustomFrameRateValueProperty;
        
        SerializedProperty m_StartFrameProperty;
        SerializedProperty m_EndFrameProperty;
        SerializedProperty m_StartTimeProperty;
        SerializedProperty m_EndTimeProperty;
        
        SerializedProperty m_SynchFrameRateProperty;

        GenericMenu m_FrameRateMenu;

        static class Styles
        {
            public static readonly GUIContent SRecordModeLabel  = new GUIContent("Record Mode");
            public static readonly GUIContent SSingleFrameLabel = new GUIContent("Frame #");
            public static readonly GUIContent SFirstFrameLabel  = new GUIContent("First frame");
            public static readonly GUIContent SLastFrameLabel   = new GUIContent("Last frame");
            public static readonly GUIContent SStartTimeLabel   = new GUIContent("Start (sec)");
            public static readonly GUIContent SEndTimeLabel     = new GUIContent("End (sec)");
            
            public static readonly GUIContent SFrameRateTitle   = new GUIContent("Frame Rate");
            public static readonly GUIContent SPlaybackLabel    = new GUIContent("Playback");
            public static readonly GUIContent STargetFPSLabel   = new GUIContent("Target Frame Rate");
            public static readonly GUIContent SMaxFPSLabel      = new GUIContent("Max Frame Rate");
            public static readonly GUIContent SSyncFPSLabel     = new GUIContent("Sync. Frame Rate");
            public static readonly GUIContent SValueLabel       = new GUIContent("Value");
        }

        void OnEnable()
        {
            if (target == null)
                return;
            
            m_RecordModeProperty = serializedObject.FindProperty("m_RecordMode");
            m_PlaybackProperty = serializedObject.FindProperty("m_FrameRatePlayback");
            m_FrameRateTypeProperty  = serializedObject.FindProperty("m_FrameRateType");
            m_CustomFrameRateValueProperty = serializedObject.FindProperty("m_CustomFrameRateValue");
            m_StartFrameProperty = serializedObject.FindProperty("m_StartFrame");
            m_EndFrameProperty = serializedObject.FindProperty("m_EndFrame");
            m_StartTimeProperty = serializedObject.FindProperty("m_StartTime");
            m_EndTimeProperty = serializedObject.FindProperty("m_EndTime");
            m_SynchFrameRateProperty = serializedObject.FindProperty("m_SynchFrameRate");
        }

        public override void OnInspectorGUI()
        {
            RecordModeGUI();
            EditorGUILayout.Separator();
            FrameRateGUI();
        }

        public bool RecordModeGUI()
        {           
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(m_RecordModeProperty, Styles.SRecordModeLabel);

            ++EditorGUI.indentLevel;
            
            switch ((RecordMode)m_RecordModeProperty.enumValueIndex)
            {
                case RecordMode.Manual:
                {
                    // Nothing
                    break;
                }
                    
                case RecordMode.SingleFrame:
                {
                    EditorGUILayout.PropertyField(m_StartFrameProperty, Styles.SSingleFrameLabel);
                    m_EndFrameProperty.intValue = m_StartFrameProperty.intValue;
                    break;
                }
                    
                case RecordMode.FrameInterval:
                {
                    EditorGUILayout.PropertyField(m_StartFrameProperty, Styles.SFirstFrameLabel);
                    EditorGUILayout.PropertyField(m_EndFrameProperty, Styles.SLastFrameLabel);
                    break;
                }
                    
                case RecordMode.TimeInterval:
                {
                    EditorGUILayout.PropertyField(m_StartTimeProperty, Styles.SStartTimeLabel);
                    EditorGUILayout.PropertyField(m_EndTimeProperty, Styles.SEndTimeLabel);
                    break;
                }
                    
            }
            
            --EditorGUI.indentLevel;            
            
            serializedObject.ApplyModifiedProperties();
            
            return GUI.changed;
        }
        
        public bool FrameRateGUI()
        {           
            serializedObject.Update();
            
            EditorGUILayout.LabelField(Styles.SFrameRateTitle);
            
            ++EditorGUI.indentLevel;
            
            EditorGUILayout.PropertyField(m_PlaybackProperty, Styles.SPlaybackLabel);

            var variableFPS = m_PlaybackProperty.enumValueIndex == (int) FrameRatePlayback.Variable;
            
            EditorGUILayout.PropertyField(m_FrameRateTypeProperty, variableFPS ? Styles.SMaxFPSLabel : Styles.STargetFPSLabel);

            if (m_FrameRateTypeProperty.enumValueIndex == (int) FrameRateType.FR_CUSTOM)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CustomFrameRateValueProperty, Styles.SValueLabel);
                --EditorGUI.indentLevel;
            }
            
            if (variableFPS)
            {
                EditorGUILayout.PropertyField(m_SynchFrameRateProperty, Styles.SSyncFPSLabel);       
            }
            
            --EditorGUI.indentLevel;
            
            serializedObject.ApplyModifiedProperties();

            return GUI.changed;
        }
    }
}