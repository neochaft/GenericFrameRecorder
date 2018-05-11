﻿using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Serializable]
    [RecorderSettings(typeof(AnimationRecorder), "Animation Clip", "animation_recorder")]
    public class AnimationRecorderSettings : RecorderSettings
    {
        [SerializeField] AnimationInputSettings m_AnimationInputSettings = new AnimationInputSettings();
   
        public AnimationRecorderSettings()
        {
            var goWildcard = FileNameGenerator.GeneratePattern("GameObject");
           
            fileNameGenerator.AddWildcard(goWildcard, GameObjectNameResolver);

            fileNameGenerator.forceAssetsFolder = true;
            fileNameGenerator.root = OutputPath.Root.AssetsFolder;
            fileNameGenerator.fileName = "animation_" + goWildcard + "_" + FileNameGenerator.DefaultWildcard.Take;            
        }

        string GameObjectNameResolver(RecordingSession session)
        {
            var go = m_AnimationInputSettings.gameObject.Resolve(SceneHook.GetRecorderBindings());
            return go != null ? go.name : "None";
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.WindowsEditor;
            }
        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get { yield return m_AnimationInputSettings; }
        }

        public override string extension
        {
            get { return "anim"; }
        }

        public override Vector2 resolution
        {
            get { return Vector2.zero; }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            
            if (m_AnimationInputSettings.gameObject.Resolve(SceneHook.GetRecorderBindings()) == null)
            {
                ok = false;
                errors.Add("No input object set");
            }

            return ok; 
        }
        
        public override void OnAfterDuplicate()
        {
            m_AnimationInputSettings.DuplicateExposedReference();
        }
        
        void OnDestroy()
        {
            m_AnimationInputSettings.ClearExposedReference();
        }
    }
}