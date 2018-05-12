using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Game View")]
    [Serializable]
    public class GameViewInputSettings : ImageInputSettings
    {
        public override Type inputType
        {
            get { return typeof(GameViewInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {   
            return true;
        }
    }
}