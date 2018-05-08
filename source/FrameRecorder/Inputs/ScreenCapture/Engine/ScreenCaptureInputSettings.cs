using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Recorder.Input
{
    [DisplayName("Game View")]
    [Serializable]
    public class ScreenCaptureInputSettings : ImageInputSettings
    {
        public override Type inputType
        {
            get { return typeof(ScreenCaptureInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {   
//            #if UNITY_EDITOR
//            var prefs = UnityEngine.Recorder.RecorderSettingsPrefs.instance;
//            
//            foreach (var recorder in prefs.recorderSettings)
//            {
//                if (recorder.inputsSettings.Where(inputSetting => inputSetting != this).OfType<ScreenCaptureInputSettings>().Any())
//                {
//                    errors.Add("Game View usage is conflicting with recorder '" + recorder.name + "'");
//                    return false;
//                }
//            }
//            #endif
            
            return true;
        }
    }
}