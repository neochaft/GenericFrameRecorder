using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Recorder.Input
{
    [DisplayName("Audio")]
    [Serializable]
    public class AudioInputSettings : RecorderInputSetting
    {
        public bool preserveAudio = true;

        public override Type inputType
        {
            get { return typeof(AudioInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            return true;
        }
    }
}