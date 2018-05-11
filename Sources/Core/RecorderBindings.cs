using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Recorder
{     
    [ExecuteInEditMode]
    public class RecorderBindings : MonoBehaviour, IExposedPropertyTable
    {
        [Serializable]
        class PropertyObjects : SerializedDictionary<PropertyName, UnityObject> { }
        
        [SerializeField] PropertyObjects m_References = new PropertyObjects();
        
        public void SetReferenceValue(PropertyName id, UnityObject value)
        {
            m_References.dictionary[id] = value;
        }
        
        public UnityObject GetReferenceValue(PropertyName id, out bool idValid)
        {
            UnityObject value;
            if (m_References.dictionary.TryGetValue(id, out value))
            {
                idValid = true;
                return value;
            }

            idValid = false;
            return null;
        }
        
        public bool HasReferenceValue(PropertyName id)
        {
            return m_References.dictionary.ContainsKey(id);
        }

        public void ClearReferenceValue(PropertyName id)
        {
            if (m_References.dictionary.ContainsKey(id))
                m_References.dictionary.Remove(id);
        }

        public void Duplicate(PropertyName src, PropertyName dst)
        {
            if (m_References.dictionary.ContainsKey(src))
                m_References.dictionary[dst] = m_References.dictionary[src];
        }
    }
}