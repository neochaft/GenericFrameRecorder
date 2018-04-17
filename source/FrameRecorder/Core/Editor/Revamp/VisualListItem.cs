using System;
using System.Collections.Generic;
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
        
        readonly List<T> m_SelectedItems = new List<T>();

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
            m_SelectedItems.Clear();
            
            foreach (var item in itemList)
                Add(item);
        }
        
        public IEnumerable<T> items
        {
            get
            {
                foreach (var item in m_ScrollView)
                    yield return (T) item;
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
            ClearSelection();
            SetSelected(item, true);
            
            if (OnSelectionChanged != null)
                OnSelectionChanged.Invoke();
        }
        
        public void Remove(T item)
        {
            m_SelectedItems.Remove(item);
            m_ScrollView.Remove(item);
        }

        void ClearSelection()
        {
            m_SelectedItems.Clear();
        }
        
        void SetSelected(T item, bool select)
        {
            if (select)
            {
                if (!m_SelectedItems.Contains(item));
                    m_SelectedItems.Add(item);
            }
            else
            {
                m_SelectedItems.Remove(item);    
            }
        }

        public bool IsSelected(T item)
        {
            return m_SelectedItems.Contains(item);
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
            var alreadySelected = IsSelected(item);
            
            var selectionChanged = false;
            
            if (evt.modifiers == EventModifiers.None)
            {
                if (m_SelectedItems.Count == 1 && alreadySelected)
                {
                    if (OnItemRename != null)
                        OnItemRename.Invoke(item);
                }
                else
                {
                    ClearSelection();
                    SetSelected(item, true);
                    
                    selectionChanged = true;
                }
            }
            else if (evt.commandKey || evt.ctrlKey)
            {
                SetSelected(item, !alreadySelected);
                
                selectionChanged = true;
            }
            else if (evt.shiftKey)
            {
                if (!alreadySelected)
                {
                    SetSelected(item, true);
                    selectionChanged = true;
                }
            }
            else if (evt.altKey)
            {
                if (alreadySelected)
                {
                    SetSelected(item, false);
                    selectionChanged = true;
                }
            }

            if (selectionChanged && OnSelectionChanged != null)
                OnSelectionChanged.Invoke();
            
            if (evt.modifiers == EventModifiers.None && evt.button == (int) MouseButton.RightMouse)
            {
                if (OnItemContextMenu != null)
                    OnItemContextMenu.Invoke(item);
            }
            
            evt.StopImmediatePropagation();
        }
    }
}
