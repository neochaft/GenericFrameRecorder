using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    class RecorderItem : VisualElement
    {
        public RecorderSettings settings { get; private set; }
        public RecorderEditor editor { get; private set; }
    
        EditableLabel m_EditableLabel;
        Image m_Icon;

        bool m_Selected;
        
        public bool IsItemSelected()
        {
            return m_Selected;
        }
        
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
            prefs.SetRecorderEnabled(settings, value);
            m_EditableLabel.SetLabelEnabled(value);
            if (value)
                RemoveFromClassList("disabled");
            else
                AddToClassList("disabled");
        }

        static readonly Dictionary<string, Texture2D> s_IconCache = new Dictionary<string, Texture2D>();
        
        public RecorderItem(RecorderSettingsPrefs prefs, RecorderSettings recorderSettings, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
        {           
            settings = recorderSettings;
    
            editor = (RecorderEditor)Editor.CreateEditor(settings);
    
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
                icon = Texture2D.whiteTexture;
            
            m_Icon = new Image
            {
                image = icon
            };

            m_Icon.SetEnabled(false);
            
            Add(m_Icon);
            
            m_EditableLabel = new EditableLabel { text = prefs.GetRecorderDisplayName(settings) };
            m_EditableLabel.OnValueChanged(newValue =>
            {
                prefs.SetRecorderDisplayName(settings, newValue);
            });
            Add(m_EditableLabel);
            
            RegisterCallback<MouseUpEvent>(InternalMouseUp);
            RegisterCallback(onRecordMouseUp);

            var recorderEnabled = prefs.IsRecorderEnabled(settings);
            toggle.on = recorderEnabled;
            SetItemEnabled(prefs, recorderEnabled);
        }
    
        void InternalMouseUp(MouseUpEvent evt)
        {
            if (m_Selected && evt.clickCount == 1 && evt.button == (int) MouseButton.LeftMouse)
            {
                evt.StopImmediatePropagation();
                m_EditableLabel.StartEditing();
            }
        }
    }
}