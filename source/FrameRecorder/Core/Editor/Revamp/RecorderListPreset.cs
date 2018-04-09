using System;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    class RecorderListPreset : ScriptableObject
    {
        [SerializeField] Preset m_Model;
        [SerializeField] List<Preset> m_RecorderPresets = new List<Preset>();
        
        public Preset model
        {
            get { return m_Model; }
        }
        
        public IEnumerable<Preset> recorderPresets
        {
            get { return m_RecorderPresets; }
        }

        public static void SaveAtPath(RecorderSettingsPrefs model, string path)
        {
            var data = CreateInstance<RecorderListPreset>();
            
            var copy = Instantiate(model);
            copy.name = model.name;
            copy.ClearRecorderSettings();

            var p = new Preset(copy) {name = model.name};
            data.m_Model = p;
            
            foreach (var recorder in model.recorders)
            {
                var rp = new Preset(recorder) { name = recorder.name };
                data.m_RecorderPresets.Add(rp);
            }
            
            //var preset = new Preset(data);
            //AssetDatabase.CreateAsset(preset, "Assets/test.preset");

            var preset = data; //new Preset(data);
            AssetDatabase.CreateAsset(preset, path); //AssetDatabase.CreateAsset(preset, "Assets/test.preset");
            
            foreach (var rp in data.m_RecorderPresets)
                AddHiddenObjectToAsset(rp, preset);
            
            AddHiddenObjectToAsset(p, preset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
                
        public void AppyTo(RecorderSettingsPrefs prefs)
        {
            prefs.ReleaseRecorderSettings();
            
            m_Model.ApplyTo(prefs);
            
            foreach (var rp in m_RecorderPresets)
            {
                var r = (RecorderSettings) CreateFromPreset(rp);
                prefs.AddRecorder(r, r.name);
            }
            
            prefs.Reload();
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