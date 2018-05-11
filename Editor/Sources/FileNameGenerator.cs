using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Recorder
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

        [SerializeField] OutputPath m_Path = new OutputPath();
        [SerializeField] string m_FileName;
        
        readonly List<Wildcard> m_Wildcards;
        
        public IEnumerable<Wildcard> wildcards
        {
            get { return m_Wildcards; }
        }
        
        public string fileName {
            get { return m_FileName;}
            set { m_FileName = value;  }
        }

        public OutputPath.Root root
        {
            get { return m_Path.root; }
            set { m_Path.root = value; }
        }
        
        public string leaf
        {
            get { return m_Path.leaf; }
            set { m_Path.leaf = value; }
        }

        public bool forceAssetsFolder
        {
            get { return m_Path.forceAssetsFolder; }
            set { m_Path.forceAssetsFolder = value; }
        }

        readonly RecorderSettings m_RecorderSettings;
        
        public static class DefaultWildcard
        {
            public static readonly string Time = GeneratePattern("Time");
            public static readonly string Take = GeneratePattern("Take");
            public static readonly string Date = GeneratePattern("Date");
            public static readonly string Project = GeneratePattern("Project");
            public static readonly string Product = GeneratePattern("Product");
            public static readonly string Scene = GeneratePattern("Scene");
            public static readonly string Resolution = GeneratePattern("Resolution");
            public static readonly string Frame = GeneratePattern("Frame");
            public static readonly string Extension = GeneratePattern("Extension");
        }
        
        public FileNameGenerator(RecorderSettings recorderSettings)
        {
            m_RecorderSettings = recorderSettings;
            
            m_Wildcards = new List<Wildcard>
            {
                new Wildcard(DefaultWildcard.Time, TimeResolver),
                new Wildcard(DefaultWildcard.Take, TakeResolver),
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
            var date = session != null ? session.sessionStartTS : DateTime.Now;
            return string.Format("{0:HH}h{1:mm}m", date, date);
        }
        
        string TakeResolver(RecordingSession session)
        {
            return m_RecorderSettings.take.ToString("000");
        }

        static string DateResolver(RecordingSession session)
        {
            var date = session != null ? session.sessionStartTS : DateTime.Now;
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
            if (string.IsNullOrEmpty(s_ProjectName))
            {
                var parts = Application.dataPath.Split('/');
                s_ProjectName = parts[parts.Length - 2];
            }
            
            return s_ProjectName;
        }

        static string ProductNameResolver(RecordingSession session)
        {
            return UnityEditor.PlayerSettings.productName;
        }

        public string BuildAbsolutePath(RecordingSession session)
        {
            return BuildPath(m_Path.GetFullPath(), session);
        }

        public void CreateDirectory(RecordingSession session)
        {
            var path = ApplyWildcards(m_Path.GetFullPath(), session);
            if(!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        
        string BuildFileName(RecordingSession session)
        {
            var f = ApplyWildcards(m_FileName, session);
            f = Path.GetInvalidFileNameChars().Aggregate(f, (current, c) => current.Replace(c.ToString(), string.Empty));
            
            return f + "." + ExtensionResolver(session);
        }
        
        string BuildPath(string path, RecordingSession session)
        {
            path = ApplyWildcards(path, session);
            return path + "/" + BuildFileName(session); // TODO Sanitize the \ per platform
        }

        string ApplyWildcards(string str, RecordingSession session)
        {
            foreach (var w in wildcards)
                str = str.Replace(w.pattern, w.Resolve(session));
            
            return str;
        }

    }
}
