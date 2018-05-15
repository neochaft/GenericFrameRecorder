using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(RecorderControllerSettings))]
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
            public static readonly GUIContent RecordModeLabel  = new GUIContent("Record Mode");
            public static readonly GUIContent SingleFrameLabel = new GUIContent("Frame #");
            public static readonly GUIContent FirstFrameLabel  = new GUIContent("First frame");
            public static readonly GUIContent LastFrameLabel   = new GUIContent("Last frame");
            public static readonly GUIContent StartTimeLabel   = new GUIContent("Start (sec)");
            public static readonly GUIContent EndTimeLabel     = new GUIContent("End (sec)");
            
            public static readonly GUIContent FrameRateTitle   = new GUIContent("Frame Rate");
            public static readonly GUIContent PlaybackLabel    = new GUIContent("Playback");
            public static readonly GUIContent TargetFPSLabel   = new GUIContent("Target");
            public static readonly GUIContent MaxFPSLabel      = new GUIContent("Max Frame Rate");
            public static readonly GUIContent SyncFPSLabel     = new GUIContent("Sync. Frame Rate");
            public static readonly GUIContent ValueLabel       = new GUIContent("Value");
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
            
            EditorGUILayout.PropertyField(m_RecordModeProperty, Styles.RecordModeLabel);

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
                    EditorGUILayout.PropertyField(m_StartFrameProperty, Styles.SingleFrameLabel);
                    m_EndFrameProperty.intValue = m_StartFrameProperty.intValue;
                    break;
                }
                    
                case RecordMode.FrameInterval:
                {
                    EditorGUILayout.PropertyField(m_StartFrameProperty, Styles.FirstFrameLabel);
                    EditorGUILayout.PropertyField(m_EndFrameProperty, Styles.LastFrameLabel);
                    break;
                }
                    
                case RecordMode.TimeInterval:
                {
                    EditorGUILayout.PropertyField(m_StartTimeProperty, Styles.StartTimeLabel);
                    EditorGUILayout.PropertyField(m_EndTimeProperty, Styles.EndTimeLabel);
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
            
            EditorGUILayout.LabelField(Styles.FrameRateTitle);
            
            ++EditorGUI.indentLevel;
            
            EditorGUILayout.PropertyField(m_PlaybackProperty, Styles.PlaybackLabel);

            var variableFPS = m_PlaybackProperty.enumValueIndex == (int) FrameRatePlayback.Variable;
            
            EditorGUILayout.PropertyField(m_FrameRateTypeProperty, variableFPS ? Styles.MaxFPSLabel : Styles.TargetFPSLabel);

            if (m_FrameRateTypeProperty.enumValueIndex == (int) FrameRateType.FR_CUSTOM)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CustomFrameRateValueProperty, Styles.ValueLabel);
                --EditorGUI.indentLevel;
            }
            
            if (variableFPS)
            {
                EditorGUILayout.PropertyField(m_SynchFrameRateProperty, Styles.SyncFPSLabel);       
            }
            
            --EditorGUI.indentLevel;
            
            serializedObject.ApplyModifiedProperties();

            return GUI.changed;
        }
    }
}