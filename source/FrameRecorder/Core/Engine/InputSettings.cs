using System;
using System.Collections.Generic;

namespace UnityEngine.Recorder
{
    public abstract class RecorderInputSetting : ScriptableObject
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
