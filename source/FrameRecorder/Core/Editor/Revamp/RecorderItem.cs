using System;
using System.Collections.Generic;
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
    
        Color m_Color;
        EditableLabel m_EditableLabel;

        bool m_Selected;

        public bool selected
        {
            get { return m_Selected;}
            set
            {
                m_Selected = value;
                style.backgroundColor = m_Selected ? Color.white : m_Color;
            }
        }

        static readonly Dictionary<string, Texture2D> s_IconCache = new Dictionary<string, Texture2D>();
    
        public RecorderItem(RecordersList recordersList, Type recorderType, string recorderName, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
        {
            var savedSettings = RecordersInventory.GenerateRecorderInitialSettings(recordersList, recorderType);
            savedSettings.name = recorderName;
            
            recordersList.Add(savedSettings);
    
            Init(savedSettings, iconName, onRecordMouseUp);
        }
        
        public RecorderItem(RecorderSettings savedSettings, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
        {
            Init(savedSettings, iconName, onRecordMouseUp);
        }
        
        void Init(RecorderSettings savedSettings, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
        {
            settings = savedSettings;
    
            editor = (RecorderEditor)Editor.CreateEditor(settings);
    
            style.flex = 1.0f;
            style.flexDirection = FlexDirection.Row;
    
            var toggle = new Toggle(null) { @on = settings.enabled };
            
            toggle.OnToggle(() =>
            {
                settings.enabled = toggle.on;
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
            
            var recordIcon = new Image
            {
                image = icon,
    
                style =
                {
                    backgroundColor = Color.black,
                    height = 16.0f,
                    width = 16.0f,
                }
            };
            
            Add(recordIcon);
            
            m_EditableLabel = new EditableLabel { text = settings.name };
            m_EditableLabel.OnValueChanged(newValue => settings.name = newValue);
            Add(m_EditableLabel);
            
            RegisterCallback<MouseUpEvent>(InternalMouseUp);
            RegisterCallback(onRecordMouseUp);

            m_Color = style.backgroundColor = Color.gray;
        }
    
        void InternalMouseUp(MouseUpEvent evt)
        {
            if (selected && evt.clickCount == 1 && evt.button == (int) MouseButton.LeftMouse)
            {
                evt.StopImmediatePropagation();
                m_EditableLabel.StartEditing();
            }
        }
    }
}