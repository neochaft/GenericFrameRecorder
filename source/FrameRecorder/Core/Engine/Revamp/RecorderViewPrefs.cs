using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class RecorderViewPrefs : ScriptableObject
    {
        static readonly string s_FilePath = "Assets/RecorderSettings.asset";
       
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
        
        [SerializeField] List<RecorderSettings> m_Recorders = new List<RecorderSettings>();
        
        public IEnumerable<RecorderSettings> recorders
        {
            get { return m_Recorders; }
        }
     
        public void AddRecorder(RecorderSettings s)
        {
            m_Recorders.Add(s);
        }

        public void RemoveRecorder(RecorderSettings s)
        {
            m_Recorders.Remove(s);
        }
        
        public void ReplaceRecorder(RecorderSettings s, RecorderSettings newSettings)
        {
            var i = m_Recorders.IndexOf(s);
            m_Recorders[i] = newSettings;
        }

        static RecorderViewPrefs s_Instance;
        
        public static RecorderViewPrefs instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = AssetDatabase.LoadAssetAtPath<RecorderViewPrefs>(s_FilePath);
           
                    if(s_Instance == null)
                    {
                        s_Instance = CreateInstance<RecorderViewPrefs>();
                        AssetDatabase.CreateAsset(s_Instance, s_FilePath);
                    }
                    
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                return s_Instance;
            }
        }
        
        public static void Load(RecorderListPreset recorderListPreset)
        {
            UnityHelpers.Destroy(s_Instance, true);
            
            s_Instance = Instantiate(recorderListPreset.model);
            AssetDatabase.CreateAsset(s_Instance, s_FilePath);
            
            foreach (var recorderSettings in recorderListPreset.model.recorders)
            {
                var copySettings = AssetSettingsHelper.Duplicate(recorderSettings, recorderSettings.name, s_Instance);
                    
                s_Instance.ReplaceRecorder(recorderSettings, copySettings);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void Release()
        {
            s_Instance = null;
        }
    }
}