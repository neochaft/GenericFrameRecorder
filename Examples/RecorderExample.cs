using UnityEditor;
using UnityEngine;
using Recorder;
using Recorder.Input;

public class RecorderExample : MonoBehaviour
{
    readonly RecorderState m_RecorderState = new RecorderState();
    
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
        
        prefs.AddRecorderSettings(videoRecorder, "My LowRes Recorder");

        m_RecorderState.StartRecording(prefs);
    }

    void OnDisable()
    {
        m_RecorderState.StopRecording();
    }
}
