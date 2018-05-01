using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.Recorder.Input
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
            var prefs = RecorderSettingsPrefs.instance;
            
            foreach (var recorder in prefs.recorderSettings)
            {
                if (recorder.inputsSettings.Where(inputSetting => inputSetting != this).OfType<ScreenCaptureInputSettings>().Any())
                {
                    errors.Add("Game View usage is conflicting with recorder '" + prefs.GetRecorderDisplayName(recorder) + "'");
                    return false;
                }
            }
            
            return true;
        }
    }
}