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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
                              
            EditorGUI.BeginChangeCheck();
            target.gameObject = EditorGUILayout.ObjectField("Game Object", target.gameObject, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                target.enabled = target.gameObject != null;

                if (target.gameObject != null)
                {
                    target.bindingTypeName.Add(target.gameObject.GetComponent<Component>().GetType().AssemblyQualifiedName);
                }
            }

            if (target.gameObject != null)
            {
                var compos = target.gameObject.GetComponents<Component>()
                    .Where(x => x != null)
                    .Select(x => x.GetType());
                if (target.recursive)
                {
                    compos = compos.Union(target.gameObject.GetComponentsInChildren<Component>()
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
                    for (int i=0;i<compoNames.Count;++i)                               
                    {
                        if ((flags & (1 << i )) == 1 << i )
                        {
                            target.bindingTypeName.Add(compoNames[i]);
                        }
                    }
                }
            }

            target.recursive = EditorGUILayout.Toggle("Recursive", target.recursive);   
        }
    }
}