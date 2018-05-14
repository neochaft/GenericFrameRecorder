using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    public class RecorderControllerSettings : ScriptableObject
    {
        [SerializeField] RecordMode m_RecordMode = RecordMode.Manual;
        [SerializeField] FrameRatePlayback m_FrameRatePlayback = FrameRatePlayback.Constant;
        [SerializeField] FrameRateType m_FrameRateType = FrameRateType.FR_30;
        [SerializeField] [Range(1.0f, 120.0f)] float m_CustomFrameRateValue = 30.0f;
        
        [SerializeField] int m_StartFrame;
        [SerializeField] int m_EndFrame;
        
        [SerializeField] float m_StartTime;
        [SerializeField] float m_EndTime;
        
        [SerializeField] bool m_SynchFrameRate;
        
        public RecordMode recordMode
        {
            get { return m_RecordMode; }
            set { m_RecordMode = value; }
        }

        public FrameRatePlayback frameRatePlayback
        {
            get { return m_FrameRatePlayback; }
            set { m_FrameRatePlayback = value; }
        }

        public float frameRate
        {
            get { return FrameRateHelper.ToFloat(m_FrameRateType, m_CustomFrameRateValue); }
            set
            {
                m_FrameRateType = FrameRateType.FR_CUSTOM;
                m_CustomFrameRateValue = value;
            }
        }

        public FrameRateType frameRateType
        {
            get { return m_FrameRateType; }
            set { m_FrameRateType = value; }
        }

        public float customFrameRateValue
        {
            get { return m_CustomFrameRateValue; }
            set { m_CustomFrameRateValue = value; }
        }

        public int startFrame
        {
            get { return m_StartFrame; }
            set { m_StartFrame = value; }
        }

        public int endFrame
        {
            get { return m_EndFrame; }
            set { m_EndFrame = value; }
        }

        public float startTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        public float endTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }

        public bool synchFrameRate
        {
            get { return m_SynchFrameRate; }
            set { m_SynchFrameRate = value; }
        }
        
        [SerializeField] List<RecorderSettings> m_RecorderSettings = new List<RecorderSettings>();

        string m_Path;
        
        public static RecorderControllerSettings LoadOrCreate(string path)
        {
            RecorderControllerSettings prefs;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                prefs = objs.FirstOrDefault(p => p is RecorderControllerSettings) as RecorderControllerSettings;
            }
            catch (Exception e)
            {
                Debug.LogError("Unhandled exception while loading Recorder preferences: " + e);
                prefs = null;
            }

            if (prefs == null)
            {
                prefs = CreateInstance<RecorderControllerSettings>();
                prefs.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                prefs.name = "Global Settings";
                prefs.Save();
            }
            
            prefs.m_Path = path;
            
            return prefs;
        }

        internal void ReleaseRecorderSettings()
        {
            foreach (var recorder in m_RecorderSettings)
            {
                DestroyImmediate(recorder);
            }

            ClearRecorderSettings();
        }
        
        internal void ClearRecorderSettings()
        {           
            m_RecorderSettings.Clear();
        }
        
        public IEnumerable<RecorderSettings> recorderSettings
        {
            get { return m_RecorderSettings; }
        }
     
        public void AddRecorderSettings(RecorderSettings recorder)
        {           
            AddRecorderInternal(recorder);
            
            Save();
        }

        public void RemoveRecorder(RecorderSettings recorder)
        {
            if (m_RecorderSettings.Contains(recorder))
            {
                m_RecorderSettings.Remove(recorder);
                
                Save();
            }
        }
        
        public void Save()
        {
            if (string.IsNullOrEmpty(m_Path))
                return;
            
            try
            {
                var directory = Path.GetDirectoryName(m_Path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var recordersCopy = recorderSettings.ToArray();

                var objs = new UnityObject[recordersCopy.Length + 1];
                objs[0] = this;

                for (int i = 0; i < recordersCopy.Length; ++i)
                    objs[i + 1] = recordersCopy[i];

                InternalEditorUtility.SaveToSerializedFileAndForget(objs, m_Path, true);
                
                if (Options.debugMode)
                    Debug.Log("Recorder settings saved");
            }
            catch (Exception e)
            {
                Debug.LogError("Unhandled exception while saving Recorder settings: " + e);
            }
        }

        internal void ApplyGlobalSetting(RecorderSettings recorder)
        {
            recorder.recordMode = m_RecordMode;
            recorder.frameRatePlayback = m_FrameRatePlayback;
            recorder.frameRateType = m_FrameRateType;
            recorder.customFrameRateValue = m_CustomFrameRateValue;
            recorder.startFrame = m_StartFrame;
            recorder.endFrame = m_EndFrame;
            recorder.startTime = m_StartTime;
            recorder.endTime = m_EndTime;
            recorder.synchFrameRate = m_SynchFrameRate;
            recorder.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            
            recorder.SelfAdjustSettings();
        }
        
        internal void ApplyGlobalSettingToAllRecorders()
        {
            foreach (var recorder in recorderSettings)
                ApplyGlobalSetting(recorder);
        }

        void AddRecorderInternal(RecorderSettings recorder)
        {
            ApplyGlobalSetting(recorder);
            m_RecorderSettings.Add(recorder);
        }
    }
}
