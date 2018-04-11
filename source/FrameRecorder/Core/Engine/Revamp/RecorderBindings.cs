using System;

namespace UnityEngine.Recorder
{     
    public class RecorderBindings : MonoBehaviour, IExposedPropertyTable
    {
        [Serializable]
        class PropertyObjects : SerializedDictionary<PropertyName, Object> { }
        
        [SerializeField] PropertyObjects m_References = new PropertyObjects();
        
        public void SetReferenceValue(PropertyName id, Object value)
        {
            m_References.dictionary[id] = value;
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            Object value = null;
            if (m_References.dictionary.TryGetValue(id, out value))
            {
                idValid = true;
                return value;
            }

            idValid = false;
            return null;
        }

        public void ClearReferenceValue(PropertyName id)
        {
            if (m_References.dictionary.ContainsKey(id))
                m_References.dictionary.Remove(id);
        }

    }
}