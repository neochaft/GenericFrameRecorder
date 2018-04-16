using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace UnityEngine.Recorder
{
    public enum ETags
    {           
        Time,
        Date,
        Project,
        Product,
        Scene,
        Resolution,
        Frame,
        Extension
    }
    
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
        
        public FileNameGenerator(RecorderSettings recorderSettings)
        {
            m_RecorderSettings = recorderSettings;
            
            m_Wildcards = new List<Wildcard>
            {
                new Wildcard(GetTagPattern(ETags.Time), TimeResolver),
                new Wildcard(GetTagPattern(ETags.Date), DateResolver),
                new Wildcard(GetTagPattern(ETags.Project), ProjectNameResolver),
                new Wildcard(GetTagPattern(ETags.Product), ProductNameResolver,"(editor only)"),
                new Wildcard(GetTagPattern(ETags.Scene), SceneResolver),
                new Wildcard(GetTagPattern(ETags.Resolution), ResolutionResolver),
                new Wildcard(GetTagPattern(ETags.Frame), FrameResolver),
                new Wildcard(GetTagPattern(ETags.Extension), ExtensionResolver)
            };
        }
        
        public static string GetTagPattern(ETags tag)
        {
            return "$" + tag;
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
