using UnityEditor;
using UnityEngine;

namespace Recorder.FrameCapturer
{
    [CustomEditor(typeof(PNGRecorderSettings))]
    class PngRecorderSettingsEditor : RecorderEditor
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PngEncoderSettings"), new GUIContent("Encoding"), true);
        }
    }
}
