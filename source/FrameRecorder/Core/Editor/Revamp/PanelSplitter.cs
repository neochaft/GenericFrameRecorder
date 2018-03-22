using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Recorder
{
    class PanelSplitter
    {
        public VisualElement uiElement { get; private set; }

        readonly VisualElement m_AffectedElement;

        bool m_Grabbed;
        Vector2 m_GrabbedMousePosition;

        float m_ElementOriginalWidth;

        const float k_SplitterWidth = 5.0f;

        public PanelSplitter(VisualElement affectedElement) // TODO Support more than one element
        {
            m_AffectedElement = affectedElement;
            
            uiElement = new VisualElement
            {
                style =
                {
                    backgroundColor = Color.blue,
                    cursor = UIElementsEditorUtility.CreateDefaultCursorStyle(MouseCursor.ResizeHorizontal),
                    width = k_SplitterWidth,
                    minWidth = k_SplitterWidth,
                    maxWidth = k_SplitterWidth
                }
            };
          
            uiElement.RegisterCallback<MouseDownEvent>(OnMouseDown, Capture.Capture);
            uiElement.RegisterCallback<MouseMoveEvent>(OnMouseMove, Capture.Capture);
            uiElement.RegisterCallback<MouseUpEvent>(OnMouseUp, Capture.Capture);
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != (int) MouseButton.LeftMouse)
                return;
            
            if (m_Grabbed)
                return;

            uiElement.TakeMouseCapture();

            m_Grabbed = true;
            m_GrabbedMousePosition = evt.mousePosition;
            m_ElementOriginalWidth = m_AffectedElement.style.width;
            
            evt.StopImmediatePropagation();
        }
        
        void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_Grabbed)
                return;

            var delta = evt.mousePosition.x - m_GrabbedMousePosition.x;

            var width = Mathf.Max(m_ElementOriginalWidth + delta, m_AffectedElement.style.minWidth);
          
            if (m_AffectedElement.style.maxWidth > 0.0f)
                width = Mathf.Min(width, m_AffectedElement.style.maxWidth);

            m_AffectedElement.style.width = width;
        }
        
        void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button != (int) MouseButton.LeftMouse)
                return;

            if (!m_Grabbed)
                return;

            m_Grabbed = false;
            uiElement.ReleaseMouseCapture();
            
            evt.StopImmediatePropagation();
        }
    }
}