using System;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(FileNameGenerator))]
    public class FileNameGeneratorDrawer : TargetedPropertyDrawer<FileNameGenerator>
    {
        SerializedProperty m_FileName;
        SerializedProperty m_Path;

        protected override void Initialize(SerializedProperty property)
        {
            if (target != null)
                return;
            
            base.Initialize(property);
            
            m_FileName = property.FindPropertyRelative("m_FileName");
            m_Path = property.FindPropertyRelative("m_Path");
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
            
            m_FileName.stringValue = GUI.TextField(txtRect, m_FileName.stringValue);
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            
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
                    m_FileName.stringValue = editor.text;
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
                        m_FileName.stringValue = InsertTag(pattern, m_FileName.stringValue, editor);
                        m_FileName.serializedObject.ApplyModifiedProperties();
                    });
                }
                
                menu.DropDown(tagRect);
            }

            EditorGUILayout.PropertyField(m_Path);

            var r = EditorGUILayout.GetControlRect();
            r.xMin = position.xMin;
            
            EditorGUI.SelectableLabel(r, target.BuildAbsolutePath(null));
            
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
                catch (Exception)
                {
                    // ignored
                }
            }

            return text + pattern;
        }
    }
}