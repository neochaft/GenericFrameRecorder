using System;
using System.Collections.Generic;

namespace UnityEngine.Recorder
{
    [Serializable]
    public abstract class RecorderInputSetting
    {
        public abstract Type inputType { get; }
        public abstract bool ValidityCheck(List<string> errors);

        string m_ID;

        protected void OnEnable()
        {
            if (string.IsNullOrEmpty(m_ID))
                m_ID = Guid.NewGuid().ToString();
        }

        public bool storeInScene
        {
            get { return Attribute.GetCustomAttribute(GetType(), typeof(StoreInSceneAttribute)) != null; }
        }
    }

}
