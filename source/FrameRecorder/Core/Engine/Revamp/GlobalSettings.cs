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
        
        [SerializeField] float m_StartTime;
        [SerializeField] float m_EndTime;
        
        [SerializeField] bool m_SynchFrameRate;
        
        public RecordMode recordMode { get { return m_RecordMode; } }

        public FrameRatePlayback frameRatePlayback { get { return m_FrameRatePlayback; } }

        public float frameRate
        {
            get { return FrameRateHelper.ToFloat(m_FrameRateType, m_CustomFrameRateValue); }
        }

        public int startFrame { get { return m_StartFrame; } }
        public int endFrame { get { return m_EndFrame; } }
        
        public float startTime { get { return m_StartTime; } } 
        public float endTime { get { return m_EndTime; } }

        public bool synchFrameRate { get { return m_SynchFrameRate; } }

        static GlobalSettings s_Instance;

        public static GlobalSettings instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = LoadSettings<GlobalSettings>("GlobalSettings");

                return s_Instance;
            }
        }
        
        static T LoadSettings<T>(string filename) where T : ScriptableObject
        {
            T asset = null;
            
            var candidates = AssetDatabase.FindAssets("t:" + filename);
            if (candidates.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(candidates[0]);
                asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            
            if(asset == null)
            {
                asset = CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, "Assets/" + filename + ".asset");
                AssetDatabase.Refresh();
            }

            return asset;
        }
    }
}