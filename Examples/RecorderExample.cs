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
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);
        
        // Video
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "My LowRes Recorder";
        videoRecorder.enabled = true;
        
        videoRecorder.outputFormat = VideoRecorderOutputFormat.MP4;
        videoRecorder.videoBitRateMode = VideoBitrateMode.Low;

        videoRecorder.videoInputSettings = new GameViewInputSettings
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
        
        controllerSettings.AddRecorderSettings(videoRecorder);
        
        controllerSettings.SetRecordModeToManual();
        controllerSettings.frameRate = 60.0f;


        m_RecorderController.verbose = true;
        m_RecorderController.StartRecording();
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}

#endif
