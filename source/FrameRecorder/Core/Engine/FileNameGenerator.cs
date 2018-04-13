using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            readonly Func<string> m_Resolver;

            public string pattern { get { return m_Pattern; } }
            public string label { get { return m_Label; } }

            public Wildcard(string pattern, string label, Func<string> resolver)
            {
                m_Pattern = pattern;
                m_Label = label; // + " pattern + " - " + label;
                m_Resolver = resolver;
            }

            public string Resolve()
            {
                return m_Resolver == null ? string.Empty : m_Resolver();
            }
            
//            public override string ToString()
//            {
//                return m_Label;
//            }
        }
        
        
        //public static string[] tagLabels { get; private set; }
        //public static string[] tags { get; private set; }
        static string s_ProjectName;
        
//        public string[] tagLabels
//        {
//            get { return wildcards.Values.Select(w => w.label).ToArray(); }
//        }

        public Dictionary<ETags, Wildcard> wildcards; // = new Dictionary<ETags, Wildcard>();

        public OutputPath path;
        
//        public OutputPath destinationPath
//        {
//            get { return m_Path; }
//        }
        
//        [Flags]
//        public enum ETags
//        {
//            None   = 0,
//            
//            Time = 1 << 0,
//            Date = 1 << 1,
//            Project = 1 << 2,
//            Product = 1 << 3,
//            Scene = 1 << 4,
//            Resolution = 1 << 5,
//            Frame = 1 << 6,
//            Extension = 1 << 7,
//            
//            All = Time | Date | Project | Product | Scene | Resolution | Frame | Extension,
//        }
        
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
        
        public FileNameGenerator()
        {
            wildcards = new Dictionary<ETags, Wildcard>
            {
                { ETags.Time, new Wildcard("<ts>", "Time", TimeResolver) },
                { ETags.Date, new Wildcard("<dt>", "Date", DateResolver) },
                { ETags.Project, new Wildcard("<prj>", "Project name", ProjectNameResolver) },
                { ETags.Product, new Wildcard("<prd>", "Product name (editor only)", ProductNameResolver) },
                { ETags.Scene, new Wildcard("<scn>", "Scene name", SceneResolver) },
                { ETags.Resolution, new Wildcard("<res>", "Resolution", ResolutionResolver) },
                { ETags.Frame, new Wildcard("<000>", "Frame number", FrameResolver) },
                { ETags.Extension, new Wildcard("<ext>", "Default extension", ExtensionResolver) }
            };


//            tags = new[]
//            {
//                "<ts>",  
//                "<dt>",
//                "<prj>",
//                "<prd>",
//                "<scn>",
//                "<res>",
//                "<00000>",
//                "<ext>"
//            };
//
//            tagLabels = new[]
//            {
//                "<ts>  - Time",  
//                "<dt>  - Date",
//                "<prj> - Project name",
//                "<prd> - Product name (editor only)",
//                "<scn> - Scene name",
//                "<res> - Resolution",
//                "<000> - Frame number",
//                "<ext> - Default extension"
//            };
            //m_Pattern = ;
            //m_FramePattern = ;
            //m_FramePatternDst = ;
        }

        public string TimeResolver()
        {
            //RecordingSession session
            //return string.Format("{0:HH}h{1:mm}m", session.m_SessionStartTS, session.m_SessionStartTS);
            return "1137";
        }
        
        public string DateResolver()
        {
            //RecordingSession session
            //return session.m_SessionStartTS.ToShortDateString().Replace('/', '-');
            return "42";
        }

        public string ExtensionResolver()
        {
            return "yolo";
        }

        public string ResolutionResolver()
        {
            return string.Format("{0}x{1}", 0, 0); //width, height)
        }
        
        public string SceneResolver()
        {
            return SceneManager.GetActiveScene().name;
        }
        
        public string FrameResolver()
        {
            return "FrameThat!";
        }
        
        public string ProjectNameResolver()
        {
            return s_ProjectName;
        }
        
        public string ProductNameResolver()
        {
#if UNITY_EDITOR
            return UnityEditor.PlayerSettings.productName;
#else
            return "(prd-NA)";
#endif
        }

        public static string AddTag(string pattern, ETags t)
        {
            if (!string.IsNullOrEmpty(pattern))
            {
                switch (t)
                {
                    case ETags.Frame:
                    case ETags.Extension:
                    {
                        pattern += ".";
                        break;
                    }
                    default:
                    {
                        pattern += "-";
                        break;
                    }
                }
            }

            //pattern += tags[(int)t]; // TODO

            return pattern;
        }

        public string BuildFullPath(RecordingSession session, int frame, int width, int height, string ext)
        {
            var fileName = BuildFileName(session, frame, width, height, ext);
            
            fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
            
            return path.GetFullPath() + "/" + fileName;
        }

        public string BuildFileName (RecordingSession session, int frame, int width, int height, string ext)
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
                fileName = fileName.Replace(wildcard.pattern, wildcard.Resolve());

            fileName += "." + ExtensionResolver();
            
            return fileName;
        }

    }
}
