using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Recorder;

namespace UTJ.FrameCapturer.Recorders
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
