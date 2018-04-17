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
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            var pf = new PropertyFinder<ImageRecorderSettings>(serializedObject);
            m_OutputFormat = pf.Find(w => w.m_OutputFormat);
        }

        protected override void FileTypeAndFormatGUI()
        {           
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Format"));
        }
    }
}
