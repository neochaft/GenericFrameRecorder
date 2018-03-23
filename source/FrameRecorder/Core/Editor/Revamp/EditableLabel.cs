using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Recorder
{
    class EditableLabel : VisualElement
    {
        readonly Label m_Label;
        readonly TextField m_TextField;

        bool m_IsEditing;

        Action<string> m_OnValueChangedCallback;
        
        public string text
        {
            get { return m_Label.text; }
            set { m_Label.text = value; }
        }

        public EditableLabel()
        {
            
            m_IsEditing = false;
            m_Label = new Label();
            m_TextField = new TextField();
            
            style.flex = 1.0f;
            m_TextField.style.flex = 1.0f;
            
            Add(m_Label);
            
            RegisterCallback<KeyUpEvent>(OnKeyUpCallback, Capture.Capture);
            
            m_TextField.RegisterCallback<FocusOutEvent>(OnTextFieldLostFocus);
        }

        void SetValueAndNotify(string newValue)
        {
            if (EqualityComparer<string>.Default.Equals(m_Label.text, newValue))
                return;

            if (string.IsNullOrEmpty(newValue))
                return;

            m_Label.text = newValue;
            
            if (m_OnValueChangedCallback != null)
                m_OnValueChangedCallback.Invoke(newValue);
        }

        public void OnValueChanged(Action<string> callback)
        {
            m_OnValueChangedCallback = callback;
        }

        public void StartEditing()
        {
            if (m_IsEditing)
                return;

            m_IsEditing = true;
            m_TextField.text = m_Label.text;
            Remove(m_Label);
            Add(m_TextField);
            m_TextField.focusIndex = 0;
            m_TextField.Focus();
        }
        
        public void ApplyEditing()
        {
            if (!m_IsEditing)
                return;

            SetValueAndNotify(m_TextField.text);
            
            m_IsEditing = false;
            Remove(m_TextField);
            Add(m_Label);
        }
        
        public void CancelEditing()
        {
            if (!m_IsEditing)
                return;

            m_IsEditing = false;
            Remove(m_TextField);
            Add(m_Label);
        }
        
        void OnTextFieldLostFocus(FocusOutEvent evt)
        {
            ApplyEditing();
        }

        void OnKeyUpCallback(KeyUpEvent evt)
        {
            if (!m_IsEditing)
                return;

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                ApplyEditing();
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                CancelEditing();
                evt.StopImmediatePropagation();
            }
        }
    }
}