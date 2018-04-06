using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;
using UnityEngine.Recorder;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    class RecorderSettingsPrefs : ScriptableObject
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

        [Serializable]
        class RecorderInfo
        {
            [SerializeField] string m_Path;
            [SerializeField] bool m_Enabled;
            [SerializeField] string m_DisplayName;
            
            public string path { get { return m_Path; } }

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

            public RecorderInfo(string path, string displayName, bool enabled = true)
            {
                m_Path = path;
                m_Enabled = enabled;
                m_DisplayName = displayName;
            }
        }
        
        [SerializeField] List<RecorderInfo> m_RecorderInfos = new List<RecorderInfo>();

        readonly Dictionary<RecorderSettings, RecorderInfo> m_RecorderInfosLookup = new Dictionary<RecorderSettings, RecorderInfo>();

        static readonly string s_Folder = "Library/Recorder";
        static readonly string s_Name = "recorder";
        static readonly string s_Extension = ".pref";
       
        void OnEnable()
        {
            Reload();
        }
        
        public static RecorderSettingsPrefs LoadOrCreate()
        {
            var prefPath = GetRelativePath(s_Name);

            var prefs = InternalEditorUtility.LoadSerializedFileAndForget(prefPath).FirstOrDefault() as RecorderSettingsPrefs;

            if (prefs == null)
            {
                prefs = CreateInstance<RecorderSettingsPrefs>();
                prefs.name = "Global Settings";
                prefs.Save();
            }

            return prefs;
        }
        
        public void Reload()
        {
            m_RecorderInfosLookup.Clear();
            
            foreach (var info in m_RecorderInfos.ToArray())
            {
                var path = info.path;
                var recorder = InternalEditorUtility.LoadSerializedFileAndForget(path).FirstOrDefault() as RecorderSettings;
                if (recorder != null)
                {
                    AddRecorderInternal(recorder, info);
                }
                else
                {
                    Debug.LogWarning("Cannot load recorder pref '" + path + "' for the recorder. Deleting file...");
                    
                    File.Delete(GetAbsolutePath(path));
                    m_RecorderInfos.Remove(info);
                }
            }
        }

        public void Release()
        {
            foreach (var info in m_RecorderInfos)
            {               
                File.Delete(GetAbsolutePath(info.path));   
            }

            foreach (var recorder in m_RecorderInfosLookup.Keys)
            {
                DestroyImmediate(recorder);
            }

            ClearRecorderSettings();
        }
        
        public void ClearRecorderSettings()
        {           
            m_RecorderInfos.Clear();
            m_RecorderInfosLookup.Clear();
        }
        
        public IEnumerable<RecorderSettings> recorders
        {
            get { return m_RecorderInfosLookup.Keys; }
        }
     
        public void AddRecorder(RecorderSettings s, string displayName)
        {
            var path = GetRelativePath(Guid.NewGuid().ToString());

            var info = new RecorderInfo(path, displayName);
            m_RecorderInfos.Add(info);

            AddRecorderInternal(s, info);
            
            InternalEditorUtility.SaveToSerializedFileAndForget(new UnityObject[] { s }, path, true);
            
            Save();
        }

        public void RemoveRecorder(RecorderSettings recorder)
        {
            RecorderInfo info;
            if (m_RecorderInfosLookup.TryGetValue(recorder, out info))
            {
                File.Delete(GetAbsolutePath(info.path));

                m_RecorderInfosLookup.Remove(recorder);
                m_RecorderInfos.Remove(info);
                
                Save();
            }
        }
        
        public void Save()
        {
            var fullPath = GetAbsolutePath(s_Folder);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            InternalEditorUtility.SaveToSerializedFileAndForget(new UnityObject[] { this }, GetRelativePath(s_Name), true);
        }

        public void ApplyGlobalSetting(RecorderSettings recorder)
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
        }

        public bool IsRecorderEnabled(RecorderSettings recorder)
        {
            RecorderInfo info;
            return m_RecorderInfosLookup.TryGetValue(recorder, out info) && info.enabled;
        }
        
        public void SetRecorderEnabled(RecorderSettings recorder, bool enabled)
        {
            RecorderInfo info;
            if (m_RecorderInfosLookup.TryGetValue(recorder, out info))
            {
                info.enabled = enabled;
                Save();
            }
        }
        
        public string GetRecorderDisplayName(RecorderSettings recorder)
        {
            RecorderInfo info;
            return m_RecorderInfosLookup.TryGetValue(recorder, out info) ? info.displayName : string.Empty;
        }
        
        public void SetRecorderDisplayName(RecorderSettings recorder, string displayName)
        {
            RecorderInfo info;
            if (m_RecorderInfosLookup.TryGetValue(recorder, out info))
            {
                info.displayName = recorder.name = displayName;
                Save();
            }
        }

        void AddRecorderInternal(RecorderSettings recorder, RecorderInfo info)
        {
            ApplyGlobalSetting(recorder);
            m_RecorderInfosLookup[recorder] = info;
        }

        static string GetAbsolutePath(string relativePath)
        {
            return Application.dataPath + "/../" + relativePath;
        }
        
        static string GetRelativePath(string fileName)
        {
            return s_Folder + "/" + fileName + s_Extension;
        }
    }
}
