using System;
using System.Linq;
using UnityEditor;


namespace UnityEngine.Recorder
{
    public static class AssetSettingsHelper
    {
        public static RecorderSettings Duplicate(RecorderSettings candidate, string name, Object parentObject)
        {
            var copy = Object.Instantiate(candidate);
                
            AssetDatabase.AddObjectToAsset(copy, parentObject);

            //copy.assetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(copy));
            copy.name = name;
                        
//            for (int i = 0; i < copy.inputsSettings.Count; ++i)
//            {
//                var input = copy.inputsSettings[i];
//                var inputCopy = Object.Instantiate(input);
//                inputCopy.name = Guid.NewGuid().ToString();
//                copy.inputsSettings.ReplaceAt(i, inputCopy, false);
//            }         

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return copy;
        }
        
        public static RecordersList Duplicate(RecordersList candidate, string name, Object parentObject)
        {
            var copy = Object.Instantiate(candidate);
            copy.name = name;
            
            AssetDatabase.AddObjectToAsset(copy, parentObject);

            foreach (var recorderSettings in candidate.recorders)
            {
                var copySettings = Duplicate(recorderSettings, recorderSettings.name, copy);
                    
                copy.Replace(recorderSettings, copySettings);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return copy;
        }
    }
}