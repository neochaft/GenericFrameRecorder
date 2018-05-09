using UnityEditor;
using UnityEngine;

namespace Recorder
{
    [CustomEditor(typeof(GIFRecorderSettings))]
    public class GIFRecorderSettingsEditor : RecorderEditor
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GifEncoderSettings"), new GUIContent("Encoding"), true);
        }
    }
}
