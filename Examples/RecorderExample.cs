using UnityEditor;
using UnityEngine;
using Recorder;
using Recorder.Input;

public class RecorderExample : MonoBehaviour
{
    //const string k_MenuRoot = "Tools/Recorder Examples/";
    readonly RecorderState m_RecorderState = new RecorderState();
    

    // Start / Stop Recordings
        
//    [MenuItem(k_MenuRoot + "Start Recording (using current config) _F10")]
//    static void StartRecording()
//    {
//        var recorderWindow = EditorWindow.GetWindow<RecorderWindow>()
//    }
//        
//    [MenuItem(k_MenuRoot + "Start Recording (using current config) _F10", true)]
//    static bool StartRecordingValidation()
//    {
//        return Application.isPlaying;
//    }
//        
//    [MenuItem(k_MenuRoot + "Stop Recording")]
//    static void StopRecording()
//    {
//            
//    }
//        
//    [MenuItem(k_MenuRoot + "Stop Recording")]
//    static bool StopRecordingValidation()
//    {
//        return Application.isPlaying; // && m_RecorderState.IsRecording();
//    }
        
    // Start Specific Recording types
        
    //[MenuItem(k_MenuRoot + "Video/Start Recording (Game View / Low Quality)")]
    void OnEnable()
    {
        var prefs = ScriptableObject.CreateInstance<RecorderSettingsPrefs>();

        var videoRecorder = RecordersInventory.CreateDefaultRecorderSettings<VideoRecorderSettings>();

        videoRecorder.outputFormat = VideoRecorderOutputFormat.MP4;
        videoRecorder.videoBitRateMode = VideoBitrateMode.Low;

        videoRecorder.videoInputSettings = new ScreenCaptureInputSettings
        {
            aspectRatio = ImageAspect.x16_9,
            outputResolution = ImageResolution.x240p
        };


        videoRecorder.audioInputSettings.preserveAudio = true;

        videoRecorder.fileNameGenerator.fileName = "Yolo";
        videoRecorder.fileNameGenerator.root = OutputPath.Root.Project;
        videoRecorder.fileNameGenerator.leaf = "ScriptRecordings";
        
        prefs.AddRecorder(videoRecorder, "My LowRes Recorder");

        //var recorderState = new 
        m_RecorderState.StartRecording(prefs);

        //prefs.AddRecorder(new );
        //m_State = State.WaitForPlayMode;
    }

    void OnDisable()
    {
        m_RecorderState.StopRecording();
    }
        
//    [MenuItem(k_MenuRoot + "Video/Start Recording (Game View / High Quality)")]
//    static void StartImageSequenceRecording()
//    {
//            
//    }

    
    
}
