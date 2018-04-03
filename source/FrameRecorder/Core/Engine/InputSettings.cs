using System;
using System.Collections.Generic;

namespace UnityEngine.Recorder
{
    [Serializable]
    public abstract class RecorderInputSetting
    {
        public abstract Type inputType { get; }
        public abstract bool ValidityCheck(List<string> errors);
        
        public string id;

        protected void OnEnable()
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
        }

        public bool storeInScene
        {
            get { return Attribute.GetCustomAttribute(GetType(), typeof(StoreInSceneAttribute)) != null; }
        }
    }

}
