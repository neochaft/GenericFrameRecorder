﻿using System;
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UTJ.FrameCapturer.Recorders
{
    [Serializable]
    public class UTJVideoSelector : InputSettingsSelector
    {
        [SerializeField] CBRenderTextureInputSettings m_CbRenderTextureInputSettings = new CBRenderTextureInputSettings();
        [SerializeField] RenderTextureSamplerSettings m_RenderTextureSamplerSettings = new RenderTextureSamplerSettings();
        [SerializeField] RenderTextureInputSettings m_RenderTextureInputSettings = new RenderTextureInputSettings();
        
        public UTJVideoSelector()
        {
            m_CbRenderTextureInputSettings.flipFinalOutput = true;
            m_RenderTextureSamplerSettings.flipFinalOutput = true;
        }

        public void SetMaxResolution(EImageDimension maxSupportedSize)
        {
            m_CbRenderTextureInputSettings.maxSupportedSize = maxSupportedSize;
            m_RenderTextureSamplerSettings.maxSupportedSize = maxSupportedSize;
            m_RenderTextureInputSettings.maxSupportedSize = maxSupportedSize;
        }
    }
    
    public abstract class BaseFCRecorderSettings : RecorderSettings
    {
        [SerializeField] protected UTJVideoSelector m_VideoSelector = new UTJVideoSelector();

        public override bool ValidityCheck( List<string> errors )
        {
            var ok = base.ValidityCheck(errors);
            
            if( string.IsNullOrEmpty(destinationPath.GetFullPath() ))
            {
                ok = false;
                errors.Add("Missing destination path.");
            } 
            if(  string.IsNullOrEmpty(baseFileName.pattern))
            {
                ok = false;
                errors.Add("missing file name");
            }

            return ok;
        }

        public override bool isPlatformSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || 
                       Application.platform == RuntimePlatform.WindowsPlayer ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.OSXPlayer ||
                       Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.LinuxPlayer;
            }
        }

        public override IEnumerable<RecorderInputSetting> inputsSettings
        {
            get { yield return m_VideoSelector.selected; }
        }
    }
}
