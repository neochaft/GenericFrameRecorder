using System;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(VideoRecorderSettings))]
    public class VideoRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_EncodingBitRateMode;
        //SerializedProperty m_VideoSelector;
        //SerializedProperty m_AudioSelector;       

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
            //m_VideoSelector = serializedObject.FindProperty("m_VideoSelector");
            //m_AudioSelector = serializedObject.FindProperty("m_AudioInputSettings");
        }

        protected override void OnEncodingGui()
        {
            AddProperty(m_EncodingBitRateMode, () => EditorGUILayout.PropertyField(m_EncodingBitRateMode, new GUIContent("Bitrate Mode")));
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));
        }

        protected override EFieldDisplayState GetFieldDisplayState(SerializedProperty property)
        {
            if (property.name == "captureEveryNthFrame")
                return EFieldDisplayState.Hidden;
            
            if (property.name == "m_FrameRateMode")
                return EFieldDisplayState.Disabled;

            if (property.name == "allowTransparency")
                return ((VideoRecorderSettings) target).m_OutputFormat == MediaRecorderOutputFormat.MP4 ? EFieldDisplayState.Disabled : EFieldDisplayState.Enabled;

            return base.GetFieldDisplayState(property);
        }

//        protected override void ImageRenderOptionsGUI()
//        {
//            EditorGUILayout.PropertyField(m_VideoSelector, true);
//            EditorGUILayout.PropertyField(m_AudioSelector, true);
//        }
    }
}
