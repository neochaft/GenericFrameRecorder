using System;
using UnityEditor;
using UnityEditor.Recorder;

namespace UnityEngine.Recorder
{
    public static class AssetSettingsHelper
    {
        public static RecorderSettings Duplicate(RecorderSettings candidate, string name, Object parentObject)
        {
            var copy = Object.Instantiate(candidate);
                
            AssetDatabase.AddObjectToAsset(copy, parentObject);
            
            copy.name = name;
     
//
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//
            return copy;
        }
        
        public static RecorderViewPrefs Duplicate(RecorderViewPrefs candidate, string name, Object parentObject)
        {
            var copy = Object.Instantiate(candidate);
            copy.name = name;
            
            AssetDatabase.AddObjectToAsset(copy, parentObject);

            foreach (var recorderSettings in candidate.recorders)
            {
                var copySettings = Duplicate(recorderSettings, recorderSettings.name, copy);
                    
                copy.ReplaceRecorder(recorderSettings, copySettings);
            }

            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return copy;
        }
    }
}