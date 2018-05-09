using UnityEngine;
using UnityEditor;

namespace Recorder
{
    [CustomEditor(typeof(ImageRecorderSettings))]
    public class ImageRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            var pf = new PropertyFinder<ImageRecorderSettings>(serializedObject);
            m_OutputFormat = pf.Find(w => w.outputFormat);
        }

        protected override void FileTypeAndFormatGUI()
        {           
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));
        }
    }
}
