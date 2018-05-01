using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder.Input
{
    [Serializable]
    [DisplayName("Animation")]
    public class AnimationInputSettings : RecorderInputSetting
    {
        public ExposedReference<GameObject> gameObject;
        public bool recursive = true;
                   
        [HideInInspector]
        public List<string> bindingTypeName = new List<string>();
        
        public List<Type> bindingType
        {
            get
            {
                var ret = new List<Type>(bindingTypeName.Count);
                foreach (var t in bindingTypeName)
                {
                    ret.Add(Type.GetType(t));
                }
                return ret;
            }
        }

        public override Type inputType
        {
            get { return typeof(AnimationInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (PropertyName.IsNullOrEmpty(gameObject.exposedName) && bindingType.Count > 0
                && bindingType.Any(x => typeof(MonoBehaviour).IsAssignableFrom(x) || typeof(ScriptableObject).IsAssignableFrom(x))
            )
            {
                ok = false;
                errors.Add("MonoBehaviours and ScriptableObjects are not supported inputs.");
            }

            return ok;
        }

        public void DuplicateExposedReference()
        {
            if (PropertyName.IsNullOrEmpty(gameObject.exposedName))
                return;

            var src = gameObject.exposedName;
            var dst = GUID.Generate().ToString();

            gameObject.exposedName = dst;
            
            var rb = SceneHook.GetRecorderBindings();
            if (rb != null)
                rb.Duplicate(src, dst);
        }

        public void ClearExposedReference()
        {
            if (PropertyName.IsNullOrEmpty(gameObject.exposedName))
                return;
            
            var rb = SceneHook.GetRecorderBindings();
            if (rb != null)
                rb.ClearReferenceValue(gameObject.exposedName);
        }
    }
}
