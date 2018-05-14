using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    class RecorderItem : VisualElement
    {
        public RecorderSettings settings { get; private set; }
        public Editor editor { get; private set; }

        readonly EditableLabel m_EditableLabel;
        readonly Toggle m_Toggle;

        readonly Texture2D m_RecorderIcon;
        
        static Texture2D s_ErrorIcon;
        static Texture2D s_WarningIcon;
        Texture2D m_Icon;
        
        static readonly Dictionary<string, Texture2D> s_IconCache = new Dictionary<string, Texture2D>();

        bool m_Selected;
        bool m_Disabled;
        
        public void SetItemSelected(bool value)
        {
            m_Selected = value;
            if (value)
                AddToClassList("selected");
            else
                RemoveFromClassList("selected");
        }

        public void SetItemEnabled(RecorderControllerSettings prefs, bool value)
        {
            m_Disabled = !value;
            settings.enabled = value;
            prefs.Save();
            
            m_EditableLabel.SetLabelEnabled(value);

            if (m_Toggle != null)
                m_Toggle.on = value;
            
            if (value)
                RemoveFromClassList("disabled");
            else
                AddToClassList("disabled");
        }

        public enum State
        {
            None,
            Normal,
            HasWarnings,
            HasErrors
        }

        State m_State = State.None;

        void UpdateState(bool checkForWarnings = true)
        {
            if (settings == null || settings.HasErrors())
            {
                state = State.HasErrors;
                return;
            }

            if (checkForWarnings && settings.HasWarnings())
            {
                state = State.HasWarnings;
                return;
            }

            state = State.Normal;
        }

        public State state
        {
            get { return m_State; }
            set
            {
                if (value == State.None)
                    return;
                
                if (m_State == value)
                    return;

                switch (m_State)
                {
                    case State.HasWarnings:
                        RemoveFromClassList("hasWarnings");
                        break;

                    case State.HasErrors:
                        RemoveFromClassList("hasErrors");
                        break;
                }

                switch (value)
                {
                    case State.HasWarnings:
                        AddToClassList("hasWarnings");
                        m_Icon = s_WarningIcon;
                        break;

                    case State.HasErrors:
                        AddToClassList("hasErrors");
                        m_Icon = s_ErrorIcon;
                        break;

                    case State.Normal:
                        m_Icon = m_RecorderIcon;
                        break;
                }

                m_State = value;
            }
        }
        
        public RecorderItem(RecorderControllerSettings prefs, RecorderSettings recorderSettings, string iconName)
        {           
            settings = recorderSettings;
            
            if (settings != null)
                editor = Editor.CreateEditorWithContext(new UnityObject[] { settings }, SceneHook.GetRecorderBindings(), null);
    
            style.flex = 1.0f;
            style.flexDirection = FlexDirection.Row;

            m_Toggle = new Toggle(null);
            
            m_Toggle.OnToggle(() =>
            {
                SetItemEnabled(prefs, m_Toggle.on);
            });
            
            Add(m_Toggle);

            if (s_ErrorIcon == null)
                s_ErrorIcon = EditorGUIUtility.Load("Icons/console.erroricon.sml.png") as Texture2D;
            
            if (s_WarningIcon == null)
                s_WarningIcon = EditorGUIUtility.Load("Icons/console.warnicon.sml.png") as Texture2D;
                
            if (!string.IsNullOrEmpty(iconName) && !s_IconCache.TryGetValue(iconName, out m_RecorderIcon))
                m_RecorderIcon = s_IconCache[iconName] = Resources.Load<Texture2D>(iconName);
            
            if (m_RecorderIcon == null)
                m_RecorderIcon = Texture2D.whiteTexture; // TODO Default icon
       
            UpdateState(false);
            
            var iconContainer = new IMGUIContainer(() => // UIElement Image doesn't support tint yet. Use IMGUI instead.
            {   
                var r = EditorGUILayout.GetControlRect();
                r.width = r.height = Mathf.Max(r.width, r.height);
                
                var c = GUI.color;

                Color newColor;

                if (m_Disabled)
                {
                    newColor = Color.gray;
                }
                else if (m_State != State.Normal)
                {
                    newColor = Color.white;
                }
                else
                {
                    newColor = EditorGUIUtility.isProSkin ? Color.white: Color.black;
                }

                if (!m_Selected)
                    newColor.a = 0.7f;

                GUI.color = newColor;
                
                GUI.DrawTexture(r, m_Icon);

                GUI.color = c;
            });
            
            iconContainer.AddToClassList("RecorderItemIcon");

            iconContainer.SetEnabled(false);
            
            Add(iconContainer);
            
            m_EditableLabel = new EditableLabel { text = settings.name };
            m_EditableLabel.OnValueChanged(newValue =>
            {
                settings.name = newValue;
                prefs.Save();
            });
            Add(m_EditableLabel);

            var recorderEnabled = settings.enabled;
            m_Toggle.on = recorderEnabled;
            SetItemEnabled(prefs, recorderEnabled);
        }
    
        public void StartRenaming()
        {
            m_EditableLabel.StartEditing();
        }
    }
}