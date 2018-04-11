using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.Recorder.Input;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Experimental.FrameRecorder.Input
{
    [CustomPropertyDrawer(typeof(AnimationInputSettings))]
    public class AnimationInputSettingsPropertyDrawer : InputPropertyDrawer<AnimationInputSettings>
    {
        SerializedProperty m_GameObjectExposedProperty;
        SerializedProperty m_Recursive;

        IExposedPropertyTable m_Resolver;

        string m_Id;
        
        protected override void Initialize(SerializedProperty prop)
        {
            base.Initialize(prop);

            m_GameObjectExposedProperty = prop.FindPropertyRelative("gameObject");
            m_Recursive = prop.FindPropertyRelative("recursive");

            var exposedName = m_GameObjectExposedProperty.FindPropertyRelative("exposedName");
            m_Id = exposedName.stringValue;
            
            if (PropertyName.IsNullOrEmpty(m_Id))
            {
                m_Id = GUID.Generate().ToString();
                exposedName.stringValue = m_Id;
            }
            
            m_Resolver = prop.serializedObject.context as IExposedPropertyTable;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
                              
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_GameObjectExposedProperty);
            
            var gameObject = m_GameObjectExposedProperty.exposedReferenceValue as GameObject;
            
            if (EditorGUI.EndChangeCheck())
            {
                if (m_Resolver != null)
                {
                    m_Resolver.SetReferenceValue(m_Id, gameObject);
                }
            }                     
            
            if (gameObject != null)
            {
                var compos = gameObject.GetComponents<Component>()
                    .Where(x => x != null)
                    .Select(x => x.GetType());
                if (target.recursive)
                {
                    compos = compos.Union(gameObject.GetComponentsInChildren<Component>()
                        .Where(x => x != null)
                        .Select(x => x.GetType()));
                }
                
                compos = compos.Distinct()
                    .Where(x => !typeof(MonoBehaviour).IsAssignableFrom(x) && x != typeof(Animator)) // black list
                    .ToList();
                var compoNames = compos.Select(x => x.AssemblyQualifiedName).ToList();

                int flags = 0;
                foreach (var t in target.bindingTypeName)
                {
                    var found = compoNames.IndexOf(t);
                    if (found != -1)
                        flags |= 1 << found;
                }
                
                EditorGUI.BeginChangeCheck();
                
                flags = EditorGUILayout.MaskField("Recorded Target(s)", flags, compos.Select(x => x.Name).ToArray());
                
                if (EditorGUI.EndChangeCheck())
                {
                    target.bindingTypeName = new List<string>();
                    for (int i=0; i<compoNames.Count; ++i)                               
                    {
                        if ((flags & (1 << i )) == 1 << i )
                        {
                            target.bindingTypeName.Add(compoNames[i]);
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(m_Recursive);   
        }
    }
}