using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Recorder
{
    public class RecorderInfo
    {
        public Type recorderType;
        public Type settingsType;
        public string displayName;
        public string iconName;
    }

    public static class RecordersInventory
    {
        static Dictionary<Type, RecorderInfo> s_Recorders;
        
        static IEnumerable<KeyValuePair<Type, object[]>> FindRecorders()
        {
            var attribType = typeof(RecorderSettingsAttribute);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = null;
                try
                {
                    types = a.GetTypes();
                }
                catch (Exception)
                {
                    Debug.LogError( "Failed reflecting assembly: " + a.FullName );
                    continue;
                }

                foreach (var t in types)
                {
                    var attributes = t.GetCustomAttributes(attribType, false);
                    if (attributes.Length != 0)
                        yield return new KeyValuePair<Type, object[]>(t, attributes);
                }
            }
        }

        static void Init()
        {
            if (s_Recorders != null)
                return;

            s_Recorders = new Dictionary<Type, RecorderInfo>();
            foreach (var recorder in FindRecorders())
            {
                var settingsType = recorder.Key;
                var settingsAttribs = recorder.Value;
                    
                if (settingsType == null || string.IsNullOrEmpty(settingsType.FullName))
                    continue;

                if (settingsAttribs.Length == 1)
                {
                    var settingsAttrib = (RecorderSettingsAttribute) settingsAttribs[0];

                    var info = new RecorderInfo
                    {
                        settingsType = settingsType,
                        recorderType = settingsAttrib.recorderType,
                        displayName = settingsAttrib.displayName,
                        iconName = settingsAttrib.iconName
                    };

                    s_Recorders.Add(settingsType, info);
                }
            }
        }

        public static RecorderInfo GetRecorderInfo(Type settingsType)
        {
            Init();

            if (settingsType == null || string.IsNullOrEmpty(settingsType.FullName))
                return null;
            
            return s_Recorders.ContainsKey(settingsType) ? s_Recorders[settingsType] : null;
        }

        public static List<RecorderInfo> recorderInfos
        {
            get
            {
                Init();
                return s_Recorders.Values.ToList();
            }
        }

        public static Recorder CreateDefaultRecorder(RecorderSettings recorderSettings)
        {
            Init();
            var factory = GetRecorderInfo(recorderSettings.GetType());
            if (factory != null)
            {
                var recorder = (Recorder)ScriptableObject.CreateInstance(factory.recorderType);
                recorder.Reset();
                recorder.settings = recorderSettings;
                return recorder;
            }
            
            throw new ArgumentException("No factory was registered for " + recorderSettings.GetType().Name);
        }

#if UNITY_EDITOR
        
        public static T CreateDefaultRecorderSettings<T>() where T : RecorderSettings
        {
            return CreateDefaultRecorderSettings(typeof(T)) as T;
        }
        
        public static RecorderSettings CreateDefaultRecorderSettings(Type settingsType)
        {
            Init();
            var recorderinfo = GetRecorderInfo(settingsType);
            if (recorderinfo != null)
            {
                var settings = (RecorderSettings)ObjectFactory.CreateInstance(recorderinfo.settingsType);
                settings.name = settingsType.Name;

                return settings;
            }
            
            throw new ArgumentException("No factory was registered for " + settingsType.Name);            
        }
#endif

    }
}
