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
                
            AddHiddenObjectToAsset(copy, parentObject);
            
            copy.name = name;
     
            return copy;
        }

        public static void AddHiddenObjectToAsset(Object objectToAdd, Object assetObject)
        {
            objectToAdd.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(objectToAdd, assetObject);
        }
        
        public static RecorderViewPrefs Duplicate(RecorderViewPrefs candidate, string name, Object parentObject)
        {
            var copy = Object.Instantiate(candidate);
            copy.name = name;
            
            AddHiddenObjectToAsset(copy, parentObject);

            foreach (var recorderSettings in candidate.recorders)
            {
                var copySettings = Duplicate(recorderSettings, recorderSettings.name, copy);
                    
                copy.ReplaceRecorder(recorderSettings, copySettings);
            }

            return copy;
        }
    }
}