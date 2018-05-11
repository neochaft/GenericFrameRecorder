﻿
namespace UnityEditor.Recorder.Examples
{
    public static class RecorderEditorExample
    {
        const string k_MenuRoot = "Recorder/Examples/";
        
        [MenuItem(k_MenuRoot + "Start Recording")]
        static void StartRecording()
        {
            var recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
            recorderWindow.StartRecording();
        }
        
        [MenuItem(k_MenuRoot + "Stop Recording")]
        static void StopRecording()
        {
            var recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
            recorderWindow.StopRecording();
        }
        
        // TODO Add example on how to apply a preset
    }
}
