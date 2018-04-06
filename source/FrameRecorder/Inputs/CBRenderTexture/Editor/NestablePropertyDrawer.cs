using System.Reflection;

namespace UnityEditor.Recorder
{
    public class NestablePropertyDrawer : PropertyDrawer
    {
        protected object target;

        protected virtual void Initialize(SerializedProperty prop)
        {
            if (target == null)
            {
                string[] path = prop.propertyPath.Split('.');
                target = prop.serializedObject.targetObject;
                foreach (string pathNode in path)
                {
                    target = GetSerializedField(target, pathNode).GetValue(target);
                }
            }
        }

        static FieldInfo GetSerializedField(object target, string pathNode)
        {
            return target.GetType().GetField(pathNode, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }
}