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
    
    public class RecorderListPreset : RecorderPreset<RecordersList>
    {
        public static void Save(RecordersList model, string path)
        {
            var preset = CreateInstance<RecorderListPreset>();
            AssetDatabase.CreateAsset(preset, path);

            preset.m_Model = AssetSettingsHelper.Duplicate(model, model.name, preset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}