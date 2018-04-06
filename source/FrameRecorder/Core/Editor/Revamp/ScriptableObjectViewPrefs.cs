using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditorInternal;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Timeline
{
    class ScriptableObjectViewPrefs<T> : ScriptableObject where T : ScriptableObject
    {
        static readonly string k_Extension = ".pref";

        ScriptableObject m_ActiveAsset;
        T m_ActiveViewModel;

        public ScriptableObject activeAsset
        {
            get { return m_ActiveAsset; }
        }

        public T activeViewModel
        {
            get { return m_ActiveViewModel; }
        }

        public void SetActiveAsset(ScriptableObject asset)
        {
            if (m_ActiveAsset == asset)
                return;

            if (m_ActiveAsset != null)
            {
                Save(m_ActiveAsset, m_ActiveViewModel);
            }

            Load(asset);
        }

        public T CreateNewViewModel()
        {
            T model = ScriptableObject.CreateInstance<T>();
            model.hideFlags |= HideFlags.HideAndDontSave;
            return model;
        }

        private static string AssetKey(UnityObject asset)
        {
            if (asset == null)
                return string.Empty;
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }

        protected void Save(ScriptableObject asset, T viewData)
        {
            const bool saveAsText = true;

            Debug.Assert(asset != null && viewData != null, "Saving ViewModel should always have valid assets and views");
            if (asset == null || viewData == null)
                return;

            // if there is no key the object does not exist on disk,
            // so there is no guid associated with it
            string file = AssetKey(asset);
            if (string.IsNullOrEmpty(file))
                return;

            // make sure the path exists or file write will fail
            string fullPath = Application.dataPath + "/../" + GetFilePath();
            if (!System.IO.Directory.Exists(fullPath))
                System.IO.Directory.CreateDirectory(fullPath);

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { viewData }, GetProjectRelativePath(file), saveAsText);
        }

        private string GetProjectRelativePath(string file)
        {
            return GetFilePath() + "/" + file + k_Extension;
        }

        private void Load(ScriptableObject asset)
        {
            T model = null;
            string file = AssetKey(asset);
            if (!string.IsNullOrEmpty(file))
            {
                var objects = InternalEditorUtility.LoadSerializedFileAndForget(GetProjectRelativePath(file));
                if (objects.Length > 0)
                {
                    model = objects[0] as T;
                    model.hideFlags |= HideFlags.HideAndDontSave;
                }
            }

            m_ActiveAsset = asset;
            m_ActiveViewModel = model ?? CreateNewViewModel();
        }

        private string GetFilePath()
        {
            Type type = this.GetType();
            object[] atributes = type.GetCustomAttributes(true);
            foreach (object attr in atributes)
            {
//                if (attr is FilePathAttribute)
//                {
//                    FilePathAttribute f = attr as FilePathAttribute;
//                    return f.filepath;
//                }
            }
            return "Library";
        }
    }
}
