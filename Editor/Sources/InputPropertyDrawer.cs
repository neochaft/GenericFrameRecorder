using UnityEditor;
using UnityEngine;

namespace Recorder
{
    public abstract class InputPropertyDrawer<T> : TargetedPropertyDrawer<T> where T : class
    {   
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }
    }
}
