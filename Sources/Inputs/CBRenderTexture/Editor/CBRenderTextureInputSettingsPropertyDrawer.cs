using UnityEngine;
using UnityEditor;

namespace Recorder.Input
{
    [CustomPropertyDrawer(typeof(CBRenderTextureInputSettings))]
    public class CBRenderTextureInputSettingsPropertyDrawer : InputPropertyDrawer<CBRenderTextureInputSettings>
    {
        static ImageSource m_SupportedSources = ImageSource.MainCamera | ImageSource.ActiveCameras | ImageSource.TaggedCamera;
        string[] m_MaskedSourceNames;

        SerializedProperty m_Source;
        SerializedProperty m_CameraTag;
        SerializedProperty m_RenderSize;
        SerializedProperty m_RenderAspect;
        SerializedProperty m_FlipFinalOutput;
        SerializedProperty m_IncludeUI;
        SerializedProperty m_MaxSupportedSize;

        bool m_Initialized;

        protected override void Initialize(SerializedProperty property)
        {
            if (m_Initialized)
                return;

            base.Initialize(property);
            
            m_Source = property.FindPropertyRelative("source");
            m_CameraTag = property.FindPropertyRelative("cameraTag");
            m_RenderSize = property.FindPropertyRelative("outputResolution");
            m_RenderAspect = property.FindPropertyRelative("aspectRatio");
            m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
            m_IncludeUI = property.FindPropertyRelative("captureUI");

            m_Initialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (m_MaskedSourceNames == null)
                    m_MaskedSourceNames = EnumHelper.MaskOutEnumNames<ImageSource>((int)m_SupportedSources);
                
                var index = EnumHelper.GetMaskedIndexFromEnumValue<ImageSource>(m_Source.intValue, (int)m_SupportedSources);
                index = EditorGUILayout.Popup("Source", index, m_MaskedSourceNames);

                if (check.changed)
                    m_Source.intValue = EnumHelper.GetEnumValueFromMaskedIndex<ImageSource>(index, (int)m_SupportedSources);
            }

            var inputType = (ImageSource)m_Source.intValue;
            if ((ImageSource)m_Source.intValue == ImageSource.TaggedCamera )
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CameraTag, new GUIContent("Tag"));
                --EditorGUI.indentLevel;
            }

            if (inputType != ImageSource.RenderTexture)
            {
                m_RenderSize.intValue = ResolutionSelector.Popup("Output Resolution", target.maxSupportedSize, m_RenderSize.intValue);

                if (m_RenderSize.intValue > (int)ImageResolution.Window)
                {
                    EditorGUILayout.PropertyField(m_RenderAspect, new GUIContent("Aspect Ratio"));
                }

                if(inputType == ImageSource.ActiveCameras)
                {
                    EditorGUILayout.PropertyField(m_IncludeUI, new GUIContent("Include UI"));
                }
            }

            if (Options.debugMode)
                EditorGUILayout.LabelField("Flip output", m_FlipFinalOutput.boolValue.ToString());
        }
    }
}
