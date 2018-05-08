using System.Linq;
using UnityEditor;
using UnityEngine;
using Recorder;
using Recorder.Input;

public class RecorderExample : MonoBehaviour
{
    readonly RecorderController m_RecorderController = new RecorderController();
    RecorderSettingsPrefs m_Prefs;
    float m_Time;
    
    void OnEnable()
    {
        m_Time = 0;
        
        m_Prefs = ScriptableObject.CreateInstance<RecorderSettingsPrefs>();

        var videoRecorder = ScriptableObject.CreateInstance<VideoRecorderSettings>();

        videoRecorder.outputFormat = VideoRecorderOutputFormat.MP4;
        videoRecorder.videoBitRateMode = VideoBitrateMode.Low;

        videoRecorder.videoInputSettings = new ScreenCaptureInputSettings()
        {
            aspectRatio = ImageAspect.x16_9,
            outputResolution = ImageResolution.x240p
        };

        videoRecorder.audioInputSettings.preserveAudio = true;
        
        videoRecorder.fileNameGenerator.fileName = "Yolo";
        videoRecorder.fileNameGenerator.root = OutputPath.Root.Project;
        videoRecorder.fileNameGenerator.leaf = "ScriptRecordings";
        
        m_Prefs.AddRecorderSettings(videoRecorder, "My LowRes Recorder");

        //m_RecorderState.StartRecording(prefs);
    }

    void Update()
    {
        m_Time += Time.deltaTime;

        if (m_Time >= 3.0f && m_Time < 6.0f && !m_RecorderController.IsRecording())
        {
            Debug.Log("Starting first video...");
            GetComponent<Renderer>().material.color = Color.blue;
            m_RecorderController.StartRecording(m_Prefs);
        }
        else if (m_Time >= 6.0f && m_Time < 10.0f && m_RecorderController.IsRecording())
        {
            m_RecorderController.StopRecording();
            GetComponent<Renderer>().material.color = Color.white;
            Debug.Log("First video's done");
        }
        
        if (m_Time >= 10.0f && m_Time < 15.0f && !m_RecorderController.IsRecording())
        {
            Debug.Log("Starting second video...");
            GetComponent<Renderer>().material.color = Color.red;
            m_Prefs.recorderSettings.First().fileNameGenerator.fileName = "Yolo2";
            m_RecorderController.StartRecording(m_Prefs);
        }
        else if (m_Time >= 15.0f && m_RecorderController.IsRecording())
        {
            m_RecorderController.StopRecording();
            GetComponent<Renderer>().material.color = Color.white;
            Debug.Log("Second video's done");
        }
    }

    void OnDisable()
    {
        //m_RecorderState.StopRecording();
    }
}
