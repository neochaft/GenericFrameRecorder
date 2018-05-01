using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Recorder
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
            set
            {
                m_FrameRateType = FrameRateType.FR_CUSTOM;
                m_CustomFrameRateValue = value;
            }
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
                return string.IsNullOrEmpty(m_RecorderTypeName) ? null : Type.GetType(m_RecorderTypeName);
            }
            set
            {
                m_RecorderTypeName = value == null ? string.Empty : value.AssemblyQualifiedName;
            }
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
    }
}
