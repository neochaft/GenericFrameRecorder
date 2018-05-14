#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

public class RecorderExample : MonoBehaviour
{
    RecorderController m_RecorderController;
    
    void OnEnable()
    {
        var prefs = ScriptableObject.CreateInstance<RecorderSettingsPrefs>();
        m_RecorderController = new RecorderController(prefs);
        
        // Video
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();

        videoRecorder.outputFormat = VideoRecorderOutputFormat.MP4;
        videoRecorder.videoBitRateMode = VideoBitrateMode.Low;

        videoRecorder.videoInputSettings = new GameViewInputSettings()
        {
            aspectRatio = ImageAspect.x16_9,
            outputResolution = ImageResolution.x240p
        };

        videoRecorder.audioInputSettings.preserveAudio = true;
        
        videoRecorder.fileNameGenerator.fileName = "Video" + FileNameGenerator.DefaultWildcard.Extension;
        videoRecorder.fileNameGenerator.root = OutputPath.Root.Absolute;
        videoRecorder.fileNameGenerator.leaf = "ScriptRecordings";
        
        // Animation
        // TODO
        
        
        // Image Sequence
        // TODO
        
        prefs.AddRecorderSettings(videoRecorder, "My LowRes Recorder");
        
        prefs.recordMode = RecordMode.Manual;
        prefs.frameRate = 60.0f;


        m_RecorderController.debugMode = true;
        m_RecorderController.StartRecording();
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}

#endif
