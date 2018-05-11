using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recorder
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
        
        public int take = 1;
        
        public int captureEveryNthFrame = 1;

        public RecordMode recordMode { get; set; }

        public FrameRatePlayback frameRatePlayback { get; set; }

        public float frameRate
        {
            get { return FrameRateHelper.ToFloat(frameRateType, customFrameRateValue); }
            set
            {
                frameRateType = FrameRateType.FR_CUSTOM;
                customFrameRateValue = value;
            }
        }

        public FrameRateType frameRateType { get; set; }

        public float customFrameRateValue { get; set; }

        public int startFrame { get; set; }

        public int endFrame { get; set; }

        public float startTime { get; set; }

        public float endTime { get; set; }

        public bool synchFrameRate { get; set; }
        
        
        protected RecorderSettings()
        {
            fileNameGenerator = new FileNameGenerator(this)
            {
                root = OutputPath.Root.Project,
                leaf = "Recordings"
            };
        }

        public virtual bool ValidityCheck(List<string> errors)
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

        public abstract IEnumerable<RecorderInputSetting> inputsSettings { get; }
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
