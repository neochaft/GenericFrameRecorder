using UnityEngine;
using UnityEditor;

namespace Recorder
{
    [CustomEditor(typeof(ImageRecorderSettings))]
    class ImageRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_CaptureAlpha;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            var pf = new PropertyFinder<ImageRecorderSettings>(serializedObject);
            m_OutputFormat = pf.Find(w => w.outputFormat);
            
            m_OutputFormat = serializedObject.FindProperty("outputFormat");
            m_CaptureAlpha = serializedObject.FindProperty("captureAlpha");
        }

        protected override void FileTypeAndFormatGUI()
        {           
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));

            var outputFormat = ((ImageRecorderSettings) target).outputFormat; 
            if (outputFormat == ImageRecorderOutputFormat.PNG || outputFormat == ImageRecorderOutputFormat.EXR)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CaptureAlpha, new GUIContent("Capture Alpha"));
                --EditorGUI.indentLevel;
            }
        }
    }
}
