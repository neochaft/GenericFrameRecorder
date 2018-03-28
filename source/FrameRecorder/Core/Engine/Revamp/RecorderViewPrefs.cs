using System;
using System.Security;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class RecorderViewPrefs : ScriptableObject
    {
        static readonly string s_FilePath = "Assets/recorderSettings.asset";

        [SerializeField] GlobalSettings m_GlobalSettings;
        [SerializeField] RecordersList m_RecordersList;

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

                    if (s_Instance.m_GlobalSettings == null)
                    {
                        s_Instance.m_GlobalSettings = CreateInstance<GlobalSettings>();
                        s_Instance.m_GlobalSettings.name = "globalSettings";
                        AssetDatabase.AddObjectToAsset(s_Instance.m_GlobalSettings, s_Instance);
                    }

                    if (s_Instance.m_RecordersList == null)
                    {
                        s_Instance.m_RecordersList = CreateInstance<RecordersList>();
                        s_Instance.m_RecordersList.name = "recordings";
                        AssetDatabase.AddObjectToAsset(s_Instance.m_RecordersList, s_Instance);
                    }
                    
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                return s_Instance;
            }
        }

        public static void Load(RecorderListPreset recorderListPreset)
        {
            UnityHelpers.Destroy(s_Instance.m_RecordersList, true);
            s_Instance.m_RecordersList = AssetSettingsHelper.Duplicate(recorderListPreset.model, "recordings", s_Instance);
        }

        public static void Release()
        {
            s_Instance = null;
        }

        public static RecordersList recordersList
        {
            get { return instance.m_RecordersList; }
        }
        
        public static GlobalSettings globalSettings
        {
            get { return instance.m_GlobalSettings; }
        }
    }
}