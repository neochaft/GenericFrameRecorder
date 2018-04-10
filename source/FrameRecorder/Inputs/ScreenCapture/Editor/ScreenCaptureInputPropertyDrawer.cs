using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(ScreenCaptureInputSettings))]
    public class ScreenCaptureInputPropertyDrawer : InputPropertyDrawer<ScreenCaptureInputSettings>
    {
        SerializedProperty m_RenderSize;
        SerializedProperty m_RenderAspect;
        ResolutionSelector m_ResSelector;

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);
            
            m_RenderSize = property.FindPropertyRelative("outputSize");
            m_RenderAspect = property.FindPropertyRelative("aspectRatio");
            m_ResSelector = new ResolutionSelector();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            m_RenderSize.intValue = m_ResSelector.OnGUI("Output Resolution", target.maxSupportedSize, m_RenderSize.intValue);

            if (m_RenderSize.intValue > (int)EImageDimension.Window)
                EditorGUILayout.PropertyField(m_RenderAspect, new GUIContent("Aspect Ratio"));
        }
    }
}