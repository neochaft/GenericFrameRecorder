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
    
    public class InputGroups : List<IEnumerable<RecorderInputSetting>>
    {
    }
    
//    public abstract class InputSettingsSelector
//    {
//        [SerializeField] protected RecorderInputSetting m_Selected;
//
//        public RecorderInputSetting selected
//        {
//            get { return m_Selected; }
//        }
//    }

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
        public FileNameGenerator baseFileName;
        public OutputPath destinationPath;
        public int captureEveryNthFrame = 1;
        
        //[SerializeField] string m_AssetID;
        [SerializeField] bool m_IsEnabled = true; // TODO Move this to the Editor namespace?

        //[SerializeField] InputSettingsList m_InputsSettings = new InputSettingsList();
        [SerializeField] string m_RecorderTypeName;
        
        public bool enabled
        {
            get { return m_IsEnabled; }
            set { m_IsEnabled = value; }
        }

        public static FrameRatePlayback frameRatePlayback
        {
            get { return RecorderViewPrefs.instance.frameRatePlayback; }
        }
        
        public double frameRate
        {
            get { return RecorderViewPrefs.instance.frameRate; }
        }
        
        public int startFrame
        {
            get { return RecorderViewPrefs.instance.startFrame; }
        }
        
        public int endFrame
        {
            get { return RecorderViewPrefs.instance.endFrame; }
        }
        
        public float startTime
        {
            get { return RecorderViewPrefs.instance.startTime; }
        }
        
        public float endTime
        {
            get { return RecorderViewPrefs.instance.endTime; }
        }
        
        public RecordMode recordMode
        {
            get { return RecorderViewPrefs.instance.recordMode; }
        }
        
        public bool synchFrameRate
        {
            get { return RecorderViewPrefs.instance.synchFrameRate; }
        }

        protected RecorderSettings()
        {
            destinationPath.root = OutputPath.ERoot.Current;
            destinationPath.leaf = "Recordings";
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

        /// <summary>
        /// Allows for recorder specific settings logic to correct/adjust settings that might be missed by it's editor.
        /// </summary>
        /// <returns>true if setting where changed</returns>
        public virtual void SelfAdjustSettings()
        {
        }

    }
}
