using System;
using System.Collections.Generic;
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
    
    public class InputGroups : List<IEnumerable<Type>>
    {
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
        public FileNameGenerator baseFileName;
        public OutputPath destinationPath;
        public int captureEveryNthFrame = 1;
        
        [SerializeField] string m_AssetID;
        [SerializeField] bool m_IsEnabled = true; // TODO Move this to the Editor namespace?

        [SerializeField] InputSettingsList m_InputsSettings = new InputSettingsList();
        [SerializeField] string m_RecorderTypeName;
        
        public bool enabled
        {
            get { return m_IsEnabled; }
            set { m_IsEnabled = value; }
        }

        public static FrameRatePlayback frameRatePlayback
        {
            get { return GlobalSettings.instance.frameRatePlayback; }
        }
        
        public double frameRate
        {
            get { return GlobalSettings.instance.frameRate; }
        }
        
        public int startFrame
        {
            get { return GlobalSettings.instance.startFrame; }
        }
        
        public int endFrame
        {
            get { return GlobalSettings.instance.endFrame; }
        }
        
        public float startTime
        {
            get { return GlobalSettings.instance.startTime; }
        }
        
        public float endTime
        {
            get { return GlobalSettings.instance.endTime; }
        }
        
        public RecordMode recordMode
        {
            get { return GlobalSettings.instance.recordMode; }
        }
        
        public bool synchFrameRate
        {
            get { return GlobalSettings.instance.synchFrameRate; }
        }

        public InputSettingsList inputsSettings
        {
            get { return m_InputsSettings; }
        }
        
        public string assetID
        {
            get { return m_AssetID; }
            set
            {
                m_AssetID = value;
                m_InputsSettings.ownerRecorderSettingsAssetId = value;
            }
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

            if (m_InputsSettings != null)
            {
                var inputErrors = new List<string>();
                if (!m_InputsSettings.ValidityCheck(inputErrors))
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

        public virtual void OnEnable()
        {
            m_InputsSettings.OnEnable(m_AssetID);
            BindSceneInputSettings();
        }

        public void BindSceneInputSettings()
        {
            if (!m_InputsSettings.hasBrokenBindings)
                return;

            m_InputsSettings.Rebuild();

#if UNITY_EDITOR
            if (m_InputsSettings.hasBrokenBindings)
            {
                // only supported case is scene stored input settings are missing (for example: new scene loaded that does not contain the scene stored inputs.)
                m_InputsSettings.RepareMissingBindings();
            }
#endif

            if (m_InputsSettings.hasBrokenBindings)
                Debug.LogError("Recorder: missing input settings");
        }

        public virtual void OnDestroy()
        {
            if (m_InputsSettings != null)
                m_InputsSettings.OnDestroy();
        }

        public abstract List<RecorderInputSetting> GetDefaultInputSettings();

        protected T NewInputSettingsObj<T>() where T : class
        {
            return NewInputSettingsObj(typeof(T)) as T;
        }

        public virtual RecorderInputSetting NewInputSettingsObj(Type type)
        {
            var obj = (RecorderInputSetting) CreateInstance(type);
            obj.name = Guid.NewGuid().ToString();
            return obj;
        }

        public abstract InputGroups GetInputGroups();

        /// <summary>
        /// Allows for recorder specific settings logic to correct/adjust settings that might be missed by it's editor.
        /// </summary>
        /// <returns>true if setting where changed</returns>
        public virtual void SelfAdjustSettings()
        {
        }

    }
}
