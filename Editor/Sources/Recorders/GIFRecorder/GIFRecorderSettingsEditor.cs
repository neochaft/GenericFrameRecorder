using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(GIFRecorderSettings))]
    class GIFRecorderSettingsEditor : RecorderEditor
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GifEncoderSettings"), new GUIContent("Encoding"), true);
        }
    }
}
