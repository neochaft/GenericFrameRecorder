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

        [MenuItem("Window/Recorder/Video")]
        static void ShowRecorderWindow()
        {
            RecorderWindow.ShowAndPreselectCategory("Video");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            var pf = new PropertyFinder<VideoRecorderSettings>(serializedObject);
            m_OutputFormat = pf.Find(w => w.m_OutputFormat);
            m_EncodingBitRateMode = pf.Find(w => w.m_VideoBitRateMode);
            m_CaptureAlpha = serializedObject.FindProperty("m_CaptureAlpha");
        }

        protected override void OnEncodingGui()
        {
           EditorGUILayout.PropertyField(m_EncodingBitRateMode, new GUIContent("Bitrate Mode"));
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
