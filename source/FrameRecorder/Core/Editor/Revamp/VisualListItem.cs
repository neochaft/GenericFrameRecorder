using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace UnityEditor.Recorder
{   
    public class VisualListItem<T> : VisualElement where T : VisualElement
    {
        public event Action OnSelectionChanged;
        public event Action OnContextMenu;
        public event Action<T> OnItemContextMenu;
        public event Action<T> OnItemRename;
        
        [Serializable]
        class Selection
        {
            public int index = -1;
        }
        
        Selection m_Selection;
        
        int selectedIndex
        {
            get { return m_Selection != null ? m_Selection.index : 0; }
            
            set
            {
                if (m_Selection == null)
                    return;
               
                m_Selection.index = value;
                
                if (OnSelectionChanged != null)
                    OnSelectionChanged.Invoke();

                SavePersistentData();
            }
        }

        readonly ScrollView m_ScrollView;
        readonly List<T> m_ItemsCache = new List<T>();

        protected VisualListItem()
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
            m_ItemsCache.Clear();
            selectedIndex = -1;
            
            foreach (var item in itemList)
                Add(item);
            
            selection = m_ItemsCache.FirstOrDefault();
        }
        
        public List<T> items
        {
            get { return m_ItemsCache; }
        }

        public T selection
        {
            get
            {
                if(selectedIndex < 0 || selectedIndex > m_ItemsCache.Count - 1)
                    return null;
                
                return m_ItemsCache[selectedIndex];
            }
            
            private set
            {
                if (selection == value)
                    return;

                selectedIndex = m_ItemsCache.IndexOf(value);
            }
        }

        void Add(T item)
        {
            item.RegisterCallback<MouseUpEvent>(OnItemMouseUp);
            m_ScrollView.Add(item);
            m_ItemsCache.Add(item);
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
            m_ItemsCache.Remove(item);

            if (selected)
                selectedIndex = Math.Min(selectedIndex, items.Count - 1);
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

        public override void OnPersistentDataReady()
        {
            base.OnPersistentDataReady();

            var key = GetFullHierarchicalPersistenceKey();

            m_Selection = GetOrCreatePersistentData<Selection>(m_Selection, key);
            
            if (OnSelectionChanged != null)
                OnSelectionChanged.Invoke();
                
        }
    }
}
