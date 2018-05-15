using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Flags]
    public enum ImageSource
    {
        ActiveCameras = 1,
        SceneView = 2,
        MainCamera = 4,
        TaggedCamera = 8,
        RenderTexture = 16,
    }

    public enum FrameRatePlayback
    {
        Constant,
        Variable,
    }

    public enum RecordMode
    {
        Manual,
        SingleFrame,
        FrameInterval,
        TimeInterval
    }

    public abstract class RecorderSettings : ScriptableObject
    {
        public FileNameGenerator fileNameGenerator;

        public bool enabled;
        
        public int take = 1;
        
        internal int captureEveryNthFrame = 1;

        internal RecordMode recordMode { get; set; }

        internal FrameRatePlayback frameRatePlayback { get; set; }

        internal float frameRate { get; set; }

        internal int startFrame { get; set; }

        internal int endFrame { get; set; }

        internal float startTime { get; set; }

        internal float endTime { get; set; }

        internal bool synchFrameRate { get; set; }
        
        protected RecorderSettings()
        {
            fileNameGenerator = new FileNameGenerator(this)
            {
                root = OutputPath.Root.Project,
                leaf = "Recordings"
            };
        }

        internal virtual bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (inputsSettings != null)
            {
                var inputErrors = new List<string>();

                var valid = inputsSettings.All(x => x.ValidityCheck(inputErrors));
                
                if (!valid)
                {
                    errors.AddRange(inputErrors);
                    ok = false;
                }
            }

            if (Math.Abs(frameRate) <= float.Epsilon)
            {
                ok = false;
                errors.Add("Invalid frame rate.");
            }

            if (captureEveryNthFrame <= 0)
            {
                ok = false;
                errors.Add("Invalid frame skip value");
            }

            if (!isPlatformSupported)
            {
                errors.Add("Current platform is not supported");
                ok  = false;
            }

            return ok;
        }

        public virtual bool isPlatformSupported
        {
            get { return true; }
        }

        internal abstract IEnumerable<RecorderInputSetting> inputsSettings { get; }
        public abstract string extension { get; }
        public abstract Vector2 resolution { get; }

        public virtual void SelfAdjustSettings()
        {
        }

        public virtual void OnAfterDuplicate()
        {
        }

        public virtual bool HasErrors()
        {
            return false;
        }

        public virtual bool HasWarnings()
        {
            return !ValidityCheck(new List<string>()); // TODO Have a better way to get warnings
        }
    }
}
