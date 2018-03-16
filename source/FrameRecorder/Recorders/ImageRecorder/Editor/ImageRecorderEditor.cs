using System;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(ImageRecorderSettings))]
    public class ImageRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_IncludeAlpha;
        
        [MenuItem("Tools/Recorder/Video")]
        static void ShowRecorderWindow()
        {
            RecorderWindow.ShowAndPreselectCategory("Video");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            var pf = new PropertyFinder<ImageRecorderSettings>(serializedObject);
            m_OutputFormat = pf.Find(w => w.m_OutputFormat);
            m_IncludeAlpha = pf.Find(w => w.m_IncludeAlpha);
        }

        protected override void OnEncodingGroupGui()
        {
            // hiding this group by not calling parent class's implementation.  
        }

        public override void OutputFormatGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));

            if (m_OutputFormat.intValue != (int) ImageRecorderOutputFormat.JPEG)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_IncludeAlpha);
                --EditorGUI.indentLevel;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        protected override EFieldDisplayState GetFieldDisplayState(SerializedProperty property)
        {
            if (property.name == "m_AllowTransparency")
            {
                return (target as ImageRecorderSettings).m_OutputFormat == ImageRecorderOutputFormat.JPEG ? EFieldDisplayState.Hidden : EFieldDisplayState.Enabled;
            }

            return base.GetFieldDisplayState(property);
        }
    }
}
