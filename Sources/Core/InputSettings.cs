using System;
using System.Collections.Generic;

namespace Recorder
{
    [Serializable]
    public abstract class RecorderInputSetting
    {
        public abstract Type inputType { get; }
        public abstract bool ValidityCheck(List<string> errors);
    }
}
