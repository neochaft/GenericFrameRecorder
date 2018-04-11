using System;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace UTJ.FrameCapturer.Recorders
{
    [CustomEditor(typeof(PNGRecorderSettings))]
    public class PngRecorderSettingsEditor : RecorderEditorBase
    {
        protected override void OnEncodingGui()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PngEncoderSettings"), new GUIContent("Encoding"), true);
        }
    }
}
