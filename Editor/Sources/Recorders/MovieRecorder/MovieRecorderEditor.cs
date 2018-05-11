using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(MovieRecorderSettings))]
    class MovieRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_EncodingBitRateMode;
        SerializedProperty m_CaptureAlpha;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            m_OutputFormat = serializedObject.FindProperty("outputFormat");
            m_CaptureAlpha = serializedObject.FindProperty("captureAlpha");
            m_EncodingBitRateMode = serializedObject.FindProperty("videoBitRateMode");           
        }

        protected override void OnEncodingGui()
        {
           EditorGUILayout.PropertyField(m_EncodingBitRateMode, new GUIContent("Quality"));
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));

            if (((MovieRecorderSettings) target).outputFormat == VideoRecorderOutputFormat.WEBM)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CaptureAlpha, new GUIContent("Capture Alpha"));
                --EditorGUI.indentLevel;
            }
        }
    }
}
