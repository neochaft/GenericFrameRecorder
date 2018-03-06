using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class GlobalSettings : ScriptableObject
    {
        [SerializeField] RecordMode m_RecordMode = RecordMode.Manual;
        [SerializeField] FrameRatePlayback m_FrameRatePlayback = FrameRatePlayback.Constant;
        [SerializeField] FrameRate m_FrameRateType = FrameRate.FR_30;
        [SerializeField] [Range(1.0f, 120.0f)] float m_CustomFrameRateValue = 30.0f;
        
        [SerializeField] int m_StartFrame;
        [SerializeField] int m_EndFrame;
        [SerializeField] int m_StartTime;
        [SerializeField] int m_EndTime;
        
        [SerializeField] bool m_SynchFrameRate;
    }
}