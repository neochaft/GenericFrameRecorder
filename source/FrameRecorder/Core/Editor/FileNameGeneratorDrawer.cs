using System;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(FileNameGenerator))]
    public class FileNameGeneratorDrawer : TargetedPropertyDrawer<FileNameGenerator>
    {
        SerializedProperty m_Pattern;
        SerializedProperty m_Path;

        protected override void Initialize(SerializedProperty property)
        {
            if (target != null)
                return;
            
            base.Initialize(property);
            
            m_Pattern = property.FindPropertyRelative("m_Pattern");
            m_Path = property.FindPropertyRelative("path");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            const float tagWidth = 77;
            var txtWidth = position.width - tagWidth - 5;
            var txtRect = new Rect(position.x, position.y, txtWidth, position.height);
            var tagRect = new Rect(position.x + txtWidth + 5, position.y, tagWidth, position.height);
            
            m_Pattern.stringValue = GUI.TextField(txtRect, m_Pattern.stringValue);
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
         
//            EditorGUILayout.LabelField(string.Format("Selected text: {0} - Pos: {1} -- {2}",
//                editor.SelectedText,
//                editor.position,
//                editor.cursorIndex));
            
            
            if (Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
            {
                if (Event.current.keyCode == KeyCode.C)
                {
                    Event.current.Use();
                    editor.Copy();
                }
                else if (Event.current.keyCode == KeyCode.V)
                {
                    Event.current.Use();
                    editor.Paste();
                    m_Pattern.stringValue = editor.text;
                }
            }

            if (EditorGUI.DropdownButton(tagRect, new GUIContent("+ Wildcards"), FocusType.Passive))
            {
                var menu = new GenericMenu();

                foreach (var w in target.wildcards)
                {
                    var pattern = w.pattern;
                    menu.AddItem(new GUIContent(w.label), false, () =>
                    {
                        m_Pattern.stringValue = InsertTag(pattern, m_Pattern.stringValue, editor);
                        m_Pattern.serializedObject.ApplyModifiedProperties();
                    });
                }
                
                menu.DropDown(tagRect);
            }

            EditorGUILayout.PropertyField(m_Path);

            var r = EditorGUILayout.GetControlRect();
            r.xMin = position.xMin;
            
            EditorGUI.SelectableLabel(r, target.BuildFullPath(null));
            
            EditorGUI.EndProperty();
        }

        static string InsertTag(string pattern, string text, TextEditor editor) //int index, string selectedText)
        {
            if (!string.IsNullOrEmpty(editor.text)) // HACK If editor is not focused on
            {
                try
                {
                    editor.ReplaceSelection(pattern);
                    return editor.text;
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            return text + pattern;
        }
    }
}