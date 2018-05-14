using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Recorder;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    public class RecorderSettingsPrefs : ScriptableObject
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

        [Serializable]
        class RecorderInfo
        {
            [SerializeField] RecorderSettings m_Recorder;
            [SerializeField] bool m_Enabled;
            [SerializeField] string m_DisplayName;

            public RecorderSettings recorder
            {
                get { return m_Recorder; }
            }
            
            public bool enabled
            {
                get { return m_Enabled; }
                set { m_Enabled = value; }
            }
            
            public string displayName
            {
                get { return m_DisplayName; }
                set { m_DisplayName = value; }
            }

            public RecorderInfo(RecorderSettings recorder, string displayName, bool enabled = true)
            {
                m_Recorder = recorder;
                m_Enabled = enabled;
                m_DisplayName = displayName;
            }
        }
        
        [Serializable]
        class RecorderInfos : SerializedDictionary<RecorderSettings, RecorderInfo> { }
        
        [SerializeField] RecorderInfos m_RecorderInfos = new RecorderInfos();

        string m_Path;
        
        public static RecorderSettingsPrefs LoadOrCreate(string path)
        {
            RecorderSettingsPrefs prefs;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                prefs = objs.FirstOrDefault(p => p is RecorderSettingsPrefs) as RecorderSettingsPrefs;
            }
            catch (Exception e)
            {
                Debug.LogError("Unhandled exception while loading Recorder preferences: " + e);
                prefs = null;
            }

            if (prefs == null)
            {
                prefs = CreateInstance<RecorderSettingsPrefs>();
                prefs.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                prefs.name = "Global Settings";
                prefs.Save();
            }
            
            prefs.m_Path = path;
            
            return prefs;
        }

        public void ReleaseRecorderSettings()
        {
            foreach (var recorder in m_RecorderInfos.dictionary.Keys)
            {
                DestroyImmediate(recorder);
            }

            ClearRecorderSettings();
        }
        
        public void ClearRecorderSettings()
        {           
            m_RecorderInfos.dictionary.Clear();
        }
        
        public IEnumerable<RecorderSettings> recorderSettings
        {
            get { return m_RecorderInfos.dictionary.Keys; }
        }
     
        public void AddRecorderSettings(RecorderSettings recorder, string displayName, bool enabled = true)
        {
            var info = new RecorderInfo(recorder, displayName, enabled);
            
            AddRecorderInternal(info);
            
            Save();
        }

        public void RemoveRecorder(RecorderSettings recorder)
        {
            RecorderInfo info;
            if (m_RecorderInfos.dictionary.TryGetValue(recorder, out info))
            {
                m_RecorderInfos.dictionary.Remove(recorder);
                
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

        public bool IsRecorderEnabled(RecorderSettings recorder)
        {
            RecorderInfo info;
            return m_RecorderInfos.dictionary.TryGetValue(recorder, out info) && info.enabled;
        }
        
        public void SetRecorderEnabled(RecorderSettings recorder, bool enabled)
        {
            RecorderInfo info;
            if (m_RecorderInfos.dictionary.TryGetValue(recorder, out info))
            {
                info.enabled = enabled;
                Save();
            }
        }
        
        public string GetRecorderDisplayName(RecorderSettings recorder)
        {
            RecorderInfo info;
            return m_RecorderInfos.dictionary.TryGetValue(recorder, out info) ? info.displayName : string.Empty;
        }
        
        public void SetRecorderDisplayName(RecorderSettings recorder, string displayName)
        {
            RecorderInfo info;
            if (m_RecorderInfos.dictionary.TryGetValue(recorder, out info))
            {
                info.displayName = displayName;
                Save();
            }
        }

        void AddRecorderInternal(RecorderInfo info)
        {
            ApplyGlobalSetting(info.recorder);
            m_RecorderInfos.dictionary[info.recorder] = info;
        }
    }
}
