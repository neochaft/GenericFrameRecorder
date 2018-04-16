using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace UnityEngine.Recorder
{
    [Serializable]
    public class FileNameGenerator
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
                m_Label = pattern;
                
                if (info != null)
                    m_Label += " " + info;
                
                m_Resolver = resolver;
            }

            public string Resolve(RecordingSession session)
            {
                return m_Resolver == null ? string.Empty : m_Resolver(session);
            }
        }
        
        static string s_ProjectName;

        public readonly Dictionary<ETags, Wildcard> wildcards;

        public OutputPath path;
        
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
            
            wildcards = new Dictionary<ETags, Wildcard>
            {
                { ETags.Time, new Wildcard("$Time", TimeResolver) },
                { ETags.Date, new Wildcard("$Date", DateResolver) },
                { ETags.Project, new Wildcard("$Project", ProjectNameResolver) },
                { ETags.Product, new Wildcard("$Product", ProductNameResolver, "(editor only)") },
                { ETags.Scene, new Wildcard("$Scene", SceneResolver) },
                { ETags.Resolution, new Wildcard("$Resolution", ResolutionResolver) }, // TODO Oh My GOD!
                { ETags.Frame, new Wildcard("$Frame", FrameResolver) },
                { ETags.Extension, new Wildcard("$Extension", ExtensionResolver) }
            };
        }

        string TimeResolver(RecordingSession session)
        {
            var date = session != null ? session.m_SessionStartTS : DateTime.Now;
            return string.Format("{0:HH}h{1:mm}m", date, date);
        }

        string DateResolver(RecordingSession session)
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
            return string.Format("{0}x{1}", m_RecorderSettings.resolution.x, m_RecorderSettings.resolution.y); // TODO Ignore if recorder does not support res
        }
        
        string SceneResolver(RecordingSession session)
        {
            return SceneManager.GetActiveScene().name;
        }
        
        string FrameResolver(RecordingSession session)
        {
            if (session != null)
                return session.frameIndex.ToString();

            return "001";
        }
        
        string ProjectNameResolver(RecordingSession session)
        {
            return s_ProjectName;
        }
        
        string ProductNameResolver(RecordingSession session)
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
//            var regEx = new Regex("(<0*>)");
//            var match = regEx.Match(pattern);
//            if (match.Success)
//            {
//                m_FramePattern = match.Value;
//                m_FramePatternDst = m_FramePattern.Substring(1,m_FramePattern.Length-2 );
//            }
//            else
//            {
//                m_FramePattern = "<0>";
//                m_FramePatternDst = "0";
//            }

            var fileName = pattern;

            foreach (var wildcard in wildcards.Values)
                fileName = fileName.Replace(wildcard.pattern, wildcard.Resolve(session));

            fileName += "." + ExtensionResolver(session);
            
            return fileName;
        }

    }
}
