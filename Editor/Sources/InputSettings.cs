using System;
using System.Collections.Generic;

namespace UnityEditor.Recorder
{
    [Serializable]
    public abstract class RecorderInputSetting
    {
        public abstract Type inputType { get; }
        public abstract bool ValidityCheck(List<string> errors);
    }
}
