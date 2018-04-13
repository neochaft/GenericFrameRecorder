using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Recorder;

namespace UnityEngine.Recorder
{
    [Flags]
    public enum EImageSource
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
    
    /// <summary>
    /// What is this: Base settings class for all Recorders.
    /// Motivation  : All recorders share a basic common set of settings and some of them are known to the 
    ///                 recording framework, so we need a base class to allow the framework to access these settings.
    /// Notes: 
    /// - Some of the fields in this class actually don't apply to ALL recorders but are so common that they are included 
    ///   here for convenience.
    /// </summary>
    public abstract class RecorderSettings : ScriptableObject
    {
        public FileNameGenerator fileNameGenerator;
        //public OutputPath destinationPath;
        
        public int captureEveryNthFrame = 1;

        [SerializeField] string m_RecorderTypeName;
        
        [SerializeField] RecordMode m_RecordMode = RecordMode.Manual;
        [SerializeField] FrameRatePlayback m_FrameRatePlayback = FrameRatePlayback.Constant;
        [SerializeField] FrameRateType m_FrameRateType = FrameRateType.FR_30;
        [SerializeField] [Range(1.0f, 120.0f)] float m_CustomFrameRateValue = 30.0f;
        
        [SerializeField] int m_StartFrame;
        [SerializeField] int m_EndFrame;
        
        [SerializeField] float m_StartTime;
        [SerializeField] float m_EndTime;
        
        [SerializeField] bool m_SynchFrameRate;

        public RecordMode recordMode
        {
            get { return m_RecordMode; }
            set { m_RecordMode = value; }
        }

        public FrameRatePlayback frameRatePlayback
        {
            get { return m_FrameRatePlayback; }
            set { m_FrameRatePlayback = value; }
        }

        public float frameRate
        {
            get { return FrameRateHelper.ToFloat(m_FrameRateType, m_CustomFrameRateValue); }
        }

        public FrameRateType frameRateType
        {
            get { return m_FrameRateType; }
            set { m_FrameRateType = value; }
        }

        public float customFrameRateValue
        {
            get { return m_CustomFrameRateValue; }
            set { m_CustomFrameRateValue = value; }
        }

        public int startFrame
        {
            get { return m_StartFrame; }
            set { m_StartFrame = value; }
        }

        public int endFrame
        {
            get { return m_EndFrame; }
            set { m_EndFrame = value; }
        }

        public float startTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        public float endTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }

        public bool synchFrameRate
        {
            get { return m_SynchFrameRate; }
            set { m_SynchFrameRate = value; }
        }
        
        protected RecorderSettings()
        {
            fileNameGenerator = new FileNameGenerator(this)
            {
                path =
                {
                    root = OutputPath.ERoot.Absolute,
                    leaf = "Recordings"
                }
            };
        }

        public Type recorderType
        {
            get
            {
                if (string.IsNullOrEmpty(m_RecorderTypeName))
                    return null;
                return Type.GetType(m_RecorderTypeName);
            }
            set { m_RecorderTypeName = value == null ? string.Empty : value.AssemblyQualifiedName; }
        }

        public virtual bool ValidityCheck( List<string> errors )
        {
            bool ok = true;

            if (inputsSettings != null)
            {
                var inputErrors = new List<string>();

                var valid = inputsSettings.All(x => x.ValidityCheck(inputErrors));
                
                if (!valid)
                {
                    errors.Add("Input settings are incorrect.");
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

        /// <summary>
        /// Allows for recorder specific settings logic to correct/adjust settings that might be missed by it's editor.
        /// </summary>
        /// <returns>true if setting where changed</returns>
        public virtual void SelfAdjustSettings()
        {
        }

        public virtual void OnAfterDuplicate()
        {
        }
    }
}
