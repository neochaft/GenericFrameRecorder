using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace UnityEditor.Recorder
{   
    public class VisualListItem<T> : VisualElement where T : VisualElement// TODO Selection persistence
    {
        public event Action OnSelectionChanged;
        public event Action OnContextMenu;
        public event Action<T> OnItemContextMenu;
        public event Action<T> OnItemRename;
        
        T m_SelectedItem;

        readonly ScrollView m_ScrollView;
        
        public VisualListItem()
        {
            m_ScrollView = new ScrollView
            {
                style =
                {
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column
                }
            };   
            
            m_ScrollView.contentContainer.style.positionLeft = 0;
            m_ScrollView.contentContainer.style.positionRight = 0;
            
            Add(m_ScrollView);
            
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        public void Reload(IEnumerable<T> itemList)
        {
            m_ScrollView.Clear();
            m_SelectedItem = null;
            
            foreach (var item in itemList)
                Add(item);

            selection = items.FirstOrDefault();
        }
        
        public IEnumerable<T> items
        {
            get
            {
                foreach (var item in m_ScrollView)
                    yield return (T) item;
            }
        }

        public T selection
        {
            get { return m_SelectedItem; }
            private set
            {
                if (m_SelectedItem == value)
                    return;

                m_SelectedItem = value;
                
                if (OnSelectionChanged != null)
                    OnSelectionChanged.Invoke();
            }
        }

        void Add(T item)
        {
            item.RegisterCallback<MouseUpEvent>(OnItemMouseUp);
            m_ScrollView.Add(item);
        }
        
        public void AddAndSelect(T item)
        {
            Add(item);
            selection = item;
        }
        
        public void Remove(T item)
        {
            var selected = selection == item;
            
            m_ScrollView.Remove(item);

            if (selected)
                selection = items.FirstOrDefault();
        }
        
        void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.clickCount != 1)
                return;
            
            if (evt.button == (int) MouseButton.RightMouse)
            {
                if (OnContextMenu != null)
                    OnContextMenu.Invoke();
            }
            
            evt.StopImmediatePropagation();
        }
        
        void OnItemMouseUp(MouseUpEvent evt)
        {           
            if (evt.clickCount != 1)
                return;

            if (evt.button != (int) MouseButton.LeftMouse && evt.button != (int) MouseButton.RightMouse)
                return;

            var item = (T) evt.currentTarget;
            var alreadySelected = selection == item;
            
            if (evt.modifiers == EventModifiers.None)
            {
                if (alreadySelected)
                {
                    if (OnItemRename != null)
                        OnItemRename.Invoke(item);
                }
                else
                {
                    selection = item;
                }
            }

            if (evt.modifiers == EventModifiers.None && evt.button == (int) MouseButton.RightMouse)
            {
                if (OnItemContextMenu != null)
                    OnItemContextMenu.Invoke(item);
            }
            
            evt.StopImmediatePropagation();
        }
    }
}
