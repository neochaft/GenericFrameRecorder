using UnityEditor;
using UnityEngine;

namespace Recorder.FrameCapturer
{
    [CustomEditor(typeof(EXRRecorderSettings))]
    public class EXRRecorderSettingsEditor : RecorderEditor
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExrEncoderSettings"), new GUIContent("Encoding"), true);
        }

    }
}
