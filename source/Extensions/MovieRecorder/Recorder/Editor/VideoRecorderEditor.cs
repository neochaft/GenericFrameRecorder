using System;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(VideoRecorderSettings))]
    public class VideoRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_EncodingBitRateMode;
        SerializedProperty m_CaptureAlpha;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            m_OutputFormat = serializedObject.FindProperty("m_OutputFormat");
            m_CaptureAlpha = serializedObject.FindProperty("m_CaptureAlpha");
            m_EncodingBitRateMode = serializedObject.FindProperty("m_VideoBitRateMode");           
        }

        protected override void OnEncodingGui()
        {
           EditorGUILayout.PropertyField(m_EncodingBitRateMode, new GUIContent("Quality"));
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));

            if (((VideoRecorderSettings) target).m_OutputFormat == MediaRecorderOutputFormat.WEBM)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CaptureAlpha, new GUIContent("Capture Alpha"));
                --EditorGUI.indentLevel;
            }
        }
    }
}
