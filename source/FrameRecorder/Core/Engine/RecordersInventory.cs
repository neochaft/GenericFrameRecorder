using System;
using System.Collections.Generic;
using UnityEngineInternal;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace UnityEngine.Recorder
{
    public class RecorderInfo
    {
        public Type recorderType;
        public Type settingsClass;
        public string displayName;
        public string iconName;
    }

    public static class RecordersInventory
    {
        static SortedDictionary<string, RecorderInfo> s_Recorders;
        
        static IEnumerable<KeyValuePair<Type, object[]>> FindRecorders()
        {
            var attribType = typeof(RecorderAttribute);
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
#if UNITY_EDITOR
            if (s_Recorders != null)
                return;

            s_Recorders = new SortedDictionary<string, RecorderInfo>();
            foreach (var recorder in FindRecorders())
                AddRecorder(recorder.Key);
#endif
        }

        static bool AddRecorder(Type recorderType)
        {
            if (recorderType == null || string.IsNullOrEmpty(recorderType.FullName))
                return false;
            
            var recorderAttribs = recorderType.GetCustomAttributes(typeof(RecorderAttribute), false);
            if (recorderAttribs.Length == 1)
            {
                var recorderAttrib = (RecorderAttribute)recorderAttribs[0];
            
                if (s_Recorders == null)
                    s_Recorders = new SortedDictionary<string, RecorderInfo>();

                var info = new RecorderInfo
                {
                    recorderType = recorderType,
                    settingsClass = recorderAttrib.settings,
                    displayName = recorderAttrib.displayName,
                    iconName = recorderAttrib.iconName
                };

                s_Recorders.Add(info.recorderType.FullName, info);

                return true;
            }
            
            Debug.LogError(string.Format("The class '{0}' does not have a FrameRecorderAttribute attached to it. ", recorderType.FullName));

            return false;
        }

        public static RecorderInfo GetRecorderInfo(Type recorderType)
        {
            Init();
            if (s_Recorders.ContainsKey(recorderType.FullName))
                return s_Recorders[recorderType.FullName];

#if UNITY_EDITOR
            return null;
#else
            if (AddRecorder(recorderType))
                return s_Recorders[recorderType.FullName];
            else
                return null;
#endif
        }

        public static List<RecorderInfo> recorderInfos
        {
            get
            {
                Init();
                return s_Recorders.Values.ToList();
            }
        }

        public static Recorder GenerateNewRecorder(Type recorderType, RecorderSettings settings)
        {
            Init();
            var factory = GetRecorderInfo(recorderType);
            if (factory != null)
            {
                var recorder = (Recorder)ScriptableObject.CreateInstance(recorderType);
                recorder.Reset();
                recorder.settings = settings;
                return recorder;
            }
            
            throw new ArgumentException("No factory was registered for " + recorderType.Name);
        }

#if UNITY_EDITOR
        public static RecorderSettings CreateDefaultRecorder(Type recorderType)
        {
            Init();
            var recorderinfo = GetRecorderInfo(recorderType);
            if (recorderinfo != null)
            {
                var settings = (RecorderSettings)ObjectFactory.CreateInstance(recorderinfo.settingsClass);
                settings.name = recorderType.Name;
                settings.recorderType = recorderType;

                return settings;
            }
            else
                throw new ArgumentException("No factory was registered for " + recorderType.Name);            
        }
#endif

    }
}
