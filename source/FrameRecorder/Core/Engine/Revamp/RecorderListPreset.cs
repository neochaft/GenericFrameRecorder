using System;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class RecorderListPreset : ScriptableObject
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

        public static void SaveAtPath(RecorderViewPrefs model, string path)
        {
            var data = CreateInstance<RecorderListPreset>();
            
            var copy = Instantiate(model);
            copy.ClearRecorders();

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
                AssetSettingsHelper.AddHiddenObjectToAsset(rp, preset);
            
            AssetSettingsHelper.AddHiddenObjectToAsset(p, preset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static RecorderViewPrefs LoadAtPath(RecorderListPreset preset, string path)
        {
            var instance = (RecorderViewPrefs)CreateFromPreset(preset.m_Model);
            AssetDatabase.CreateAsset(instance, path);

            foreach (var rp in preset.m_RecorderPresets)
            {
                var r = (RecorderSettings) CreateFromPreset(rp);
                
                instance.AddRecorder(r);
                AssetSettingsHelper.AddHiddenObjectToAsset(r, instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return instance;
        }

        static ScriptableObject CreateFromPreset(Preset preset)
        {
            var instance = CreateInstance(preset.GetTargetFullTypeName());
            preset.ApplyTo(instance);
            
            return instance;
        }
    }
}