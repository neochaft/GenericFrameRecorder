using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{    
    public class RecorderListPreset : ScriptableObject
    {
        [SerializeField] protected RecorderViewPrefs m_Model;
        
        public RecorderViewPrefs model
        {
            get { return m_Model; }
        }
        
        public static void SaveAtPath(RecorderViewPrefs model, string path)
        {
            var preset = CreateInstance<RecorderListPreset>();
            AssetDatabase.CreateAsset(preset, path);

            preset.m_Model = AssetSettingsHelper.Duplicate(model, model.name, preset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public RecorderViewPrefs LoadAtPath(string path)
        {
            var instance = Instantiate(m_Model);
            AssetDatabase.CreateAsset(instance, path);
            
            foreach (var recorderSettings in m_Model.recorders)
            {
                var copySettings = AssetSettingsHelper.Duplicate(recorderSettings, recorderSettings.name, instance);
                    
                instance.ReplaceRecorder(recorderSettings, copySettings);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return instance;
        }
    }
}