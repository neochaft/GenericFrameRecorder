using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace UnityEngine.Recorder
{   
    public class Wildcard
    {
        readonly string m_Pattern;
        readonly string m_Label;
        readonly Func<RecordingSession, string> m_Resolver;

        public string pattern { get { return m_Pattern; } }
        public string label { get { return m_Label; } }

        public Wildcard(string pattern, Func<RecordingSession, string> resolver, string info = null)
        {
            m_Pattern = pattern;
            m_Label = m_Pattern;
                
            if (info != null)
                m_Label += " " + info;
                
            m_Resolver = resolver;
        }

        public string Resolve(RecordingSession session)
        {
            return m_Resolver == null ? string.Empty : m_Resolver(session);
        }
    }
    
    [Serializable]
    public class FileNameGenerator
    {
        static string s_ProjectName;

        readonly List<Wildcard> m_Wildcards;
        
        public IEnumerable<Wildcard> wildcards
        {
            get { return m_Wildcards; }
        }

        public OutputPath path;

        [SerializeField]
        string m_Pattern;

        string m_FramePattern;
        string m_FramePatternDst;

        public string pattern {
            get { return m_Pattern;}
            set { m_Pattern = value;  }
        }

        readonly RecorderSettings m_RecorderSettings;
        
        public static class DefaultWildcard
        {
            public static readonly string Time = FileNameGenerator.GeneratePattern("Time");
            public static readonly string Date = FileNameGenerator.GeneratePattern("Date");
            public static readonly string Project = FileNameGenerator.GeneratePattern("Project");
            public static readonly string Product = FileNameGenerator.GeneratePattern("Product");
            public static readonly string Scene = FileNameGenerator.GeneratePattern("Scene");
            public static readonly string Resolution = FileNameGenerator.GeneratePattern("Resolution");
            public static readonly string Frame = FileNameGenerator.GeneratePattern("Frame");
            public static readonly string Extension = FileNameGenerator.GeneratePattern("Extension");
        }
        
        public FileNameGenerator(RecorderSettings recorderSettings)
        {
            m_RecorderSettings = recorderSettings;
            
            m_Wildcards = new List<Wildcard>
            {
                new Wildcard(DefaultWildcard.Time, TimeResolver),
                new Wildcard(DefaultWildcard.Date, DateResolver),
                new Wildcard(DefaultWildcard.Project, ProjectNameResolver),
                new Wildcard(DefaultWildcard.Product, ProductNameResolver,"(editor only)"),
                new Wildcard(DefaultWildcard.Scene, SceneResolver),
                new Wildcard(DefaultWildcard.Resolution, ResolutionResolver),
                new Wildcard(DefaultWildcard.Frame, FrameResolver),
                new Wildcard(DefaultWildcard.Extension, ExtensionResolver)
            };
        }

        public void AddWildcard(string tag, Func<RecordingSession, string> resolver)
        {
            m_Wildcards.Add(new Wildcard(tag, resolver));
        }
        
        public static string GeneratePattern(string tag)
        {
            return "<" + tag + ">";
        }
        
        static string TimeResolver(RecordingSession session)
        {
            var date = session != null ? session.m_SessionStartTS : DateTime.Now;
            return string.Format("{0:HH}h{1:mm}m", date, date);
        }

        static string DateResolver(RecordingSession session)
        {
            var date = session != null ? session.m_SessionStartTS : DateTime.Now;
            return date.ToString(CultureInfo.InvariantCulture).Replace('/', '-');
        }

        string ExtensionResolver(RecordingSession session)
        {
            return m_RecorderSettings.extension;
        }

        string ResolutionResolver(RecordingSession session)
        {
            return string.Format("{0}x{1}", m_RecorderSettings.resolution.x, m_RecorderSettings.resolution.y);
        }

        static string SceneResolver(RecordingSession session)
        {
            return SceneManager.GetActiveScene().name;
        }

        static string FrameResolver(RecordingSession session)
        {
            return session != null ? session.frameIndex.ToString() : "001";
        }

        static string ProjectNameResolver(RecordingSession session)
        {
            return s_ProjectName;
        }

        static string ProductNameResolver(RecordingSession session)
        {
#if UNITY_EDITOR
            return UnityEditor.PlayerSettings.productName;
#else
            return "(prd-NA)";
#endif
        }

        public string BuildFullPath(RecordingSession session)
        {
            var fileName = BuildFileName(session);
            
            fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
            
            return path.GetFullPath() + "/" + fileName; // TODO Sanitize the \ per platform
        }

        public string BuildFileName (RecordingSession session)
        {
            if (string.IsNullOrEmpty(s_ProjectName))
            {
#if UNITY_EDITOR
                var parts = Application.dataPath.Split('/');
                s_ProjectName = parts[parts.Length - 2];                  
#else
                s_projectName = "N/A";
#endif
            }

            var fileName = pattern;

            foreach (var w in wildcards)
                fileName = fileName.Replace(w.pattern, w.Resolve(session));

            fileName += "." + ExtensionResolver(session);
            
            return fileName;
        }

    }
}
