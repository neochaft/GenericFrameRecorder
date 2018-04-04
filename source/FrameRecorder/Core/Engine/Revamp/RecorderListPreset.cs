using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public abstract class RecorderPreset<T> : ScriptableObject where T : ScriptableObject 
    {
        [SerializeField] protected T m_Model;
        
        public T model
        {
            get { return m_Model; }
        }
    }
    
    public class RecorderListPreset : RecorderPreset<RecorderViewPrefs>
    {
        public static void Save(RecorderViewPrefs model, string path)
        {
            var preset = CreateInstance<RecorderListPreset>();
            AssetDatabase.CreateAsset(preset, path);

            preset.m_Model = AssetSettingsHelper.Duplicate(model, model.name, preset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}