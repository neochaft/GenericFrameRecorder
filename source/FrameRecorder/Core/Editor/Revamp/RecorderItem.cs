using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Recorder;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    class RecorderItem : VisualElement
    {
        public RecorderSettings settings { get; private set; }
        public Editor editor { get; private set; }

        readonly EditableLabel m_EditableLabel;
        readonly VisualElement m_Icon;

        bool m_Selected;
        bool m_Disabled;
        
        public void SetItemSelected(bool value)
        {
            m_Selected = value;
            m_Icon.SetEnabled(value);
            if (value)
                AddToClassList("selected");
            else
                RemoveFromClassList("selected");
        }

        void SetItemEnabled(RecorderSettingsPrefs prefs, bool value)
        {
            m_Disabled = !value;
            prefs.SetRecorderEnabled(settings, value);
            m_EditableLabel.SetLabelEnabled(value);
            if (value)
                RemoveFromClassList("disabled");
            else
                AddToClassList("disabled");
        }

        static readonly Dictionary<string, Texture2D> s_IconCache = new Dictionary<string, Texture2D>();
        
        public RecorderItem(RecorderSettingsPrefs prefs, RecorderSettings recorderSettings, string iconName)
        {           
            settings = recorderSettings;
            
            editor = Editor.CreateEditorWithContext(new UnityObject[] { settings }, SceneHook.GetRecorderBindings(), null);
    
            style.flex = 1.0f;
            style.flexDirection = FlexDirection.Row;

            var toggle = new Toggle(null);
            
            toggle.OnToggle(() =>
            {
                SetItemEnabled(prefs, toggle.on);
            });
            
            Add(toggle);
    
            Texture2D icon = null;
            
            if (!string.IsNullOrEmpty(iconName))
            {
                if (!s_IconCache.TryGetValue(iconName, out icon))
                {
                    icon = s_IconCache[iconName] = Resources.Load<Texture2D>(iconName);
                }
            }

            if (icon == null)
                icon = Texture2D.whiteTexture; // TODO Default icon

            m_Icon = new IMGUIContainer(() => // UIElement Image doesn't support tint yet. Use IMGUI instead.
            {   
                var r = EditorGUILayout.GetControlRect();
                r.width = r.height = Mathf.Max(r.width, r.height);
                
                var c = GUI.color;

                var newColor = m_Disabled ? Color.gray : (EditorGUIUtility.isProSkin ? Color.white : Color.black);
                
                if (!m_Selected)
                    newColor.a = 0.7f;

                GUI.color = newColor;
                
                GUI.DrawTexture(r, icon);

                GUI.color = c;
            });
            
            m_Icon.AddToClassList("RecorderItemIcon");

            m_Icon.SetEnabled(false);
            
            Add(m_Icon);
            
            m_EditableLabel = new EditableLabel { text = prefs.GetRecorderDisplayName(settings) };
            m_EditableLabel.OnValueChanged(newValue =>
            {
                prefs.SetRecorderDisplayName(settings, newValue);
            });
            Add(m_EditableLabel);

            var recorderEnabled = prefs.IsRecorderEnabled(settings);
            toggle.on = recorderEnabled;
            SetItemEnabled(prefs, recorderEnabled);
        }
    
        public void StartRenaming()
        {
            m_EditableLabel.StartEditing();
        }
    }
}