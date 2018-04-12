using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    [Serializable]
    class RecorderPresetInfo
    {
        [SerializeField] Preset m_Preset;
        [SerializeField] string m_DisplayName;
        [SerializeField] bool m_Enabled;
            
        public Preset preset
        {
            get { return m_Preset; }
        }

        public string displayName
        {
            get { return m_DisplayName; }
        }

        public bool enabled
        {
            get { return m_Enabled; }
        }

        public RecorderPresetInfo(Preset preset, string displayName, bool enabled)
        {
            m_Preset = preset;
            m_DisplayName = displayName;
            m_Enabled = enabled;
        }
    }
    
    class RecorderListPreset : ScriptableObject
    {
        [SerializeField] Preset m_Model;
        [SerializeField] List<RecorderPresetInfo> m_RecorderPresetInfos = new List<RecorderPresetInfo>();
        
        public Preset model
        {
            get { return m_Model; }
        }
        
        public Preset[] recorderPresets
        {
            get { return m_RecorderPresetInfos.Select(i => i.preset).ToArray(); }
        }

        public static void SaveAtPath(RecorderSettingsPrefs model, string path)
        {
            var data = CreateInstance<RecorderListPreset>();
            
            var copy = Instantiate(model);
            copy.name = model.name;
            
            // TODO Remove this once there's an official way to exclude some field from being save into presets
            copy.ClearRecorderSettings(); // Do not save asset references in the preset. 

            var p = new Preset(copy) { name = model.name };
            data.m_Model = p;
            
            foreach (var recorder in model.recorders)
            {
                var rp = new Preset(recorder) { name = recorder.name };
                data.m_RecorderPresetInfos.Add(new RecorderPresetInfo(rp, model.GetRecorderDisplayName(recorder), model.IsRecorderEnabled(recorder)));
            }
            
            //var preset = new Preset(data);
            //AssetDatabase.CreateAsset(preset, "Assets/test.preset");

            var preset = data; //new Preset(data);
            AssetDatabase.CreateAsset(preset, path); //AssetDatabase.CreateAsset(preset, "Assets/test.preset");
            
            foreach (var rp in data.m_RecorderPresetInfos)
                AddHiddenObjectToAsset(rp.preset, preset);
            
            AddHiddenObjectToAsset(p, preset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
                
        public void AppyTo(RecorderSettingsPrefs prefs)
        {
            prefs.ReleaseRecorderSettings();
            
            m_Model.ApplyTo(prefs);
            
            foreach (var rp in m_RecorderPresetInfos)
            {
                var r = (RecorderSettings) CreateFromPreset(rp.preset);
                prefs.AddRecorder(r, rp.displayName, rp.enabled);
            }
            
            prefs.Save();
        }

        static ScriptableObject CreateFromPreset(Preset preset)
        {
            var instance = CreateInstance(preset.GetTargetFullTypeName());
            preset.ApplyTo(instance);
            
            return instance;
        }
        
        static void AddHiddenObjectToAsset(UnityEngine.Object objectToAdd, UnityEngine.Object assetObject)
        {
            objectToAdd.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(objectToAdd, assetObject);
        }
    }
}