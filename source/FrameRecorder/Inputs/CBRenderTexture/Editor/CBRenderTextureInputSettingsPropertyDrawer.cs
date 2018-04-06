using System;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Recorder.Input
{
    // https://gist.github.com/v01pe/79db7566e2feff7ffab87676e220fd20?ts=4#file-nestablepropertydrawer-cs

    [CustomPropertyDrawer(typeof(CBRenderTextureInputSettings))]
    public class CBRenderTextureInputSettingsPropertyDrawer : NestablePropertyDrawer
    {
        static EImageSource m_SupportedSources = EImageSource.MainCamera | EImageSource.ActiveCameras | EImageSource.TaggedCamera;
        string[] m_MaskedSourceNames;
        ResolutionSelector m_ResSelector;

        SerializedProperty m_Source;
        SerializedProperty m_CameraTag;
        SerializedProperty m_RenderSize;
        SerializedProperty m_RenderAspect;
        SerializedProperty m_FlipFinalOutput;
        SerializedProperty m_Transparency;
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
            m_RenderSize = property.FindPropertyRelative("outputSize");
            m_RenderAspect = property.FindPropertyRelative("aspectRatio");
            m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
            m_Transparency = property.FindPropertyRelative("allowTransparency");
            m_IncludeUI = property.FindPropertyRelative("captureUI");

            m_ResSelector = new ResolutionSelector();

            m_Initialized = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (m_MaskedSourceNames == null)
                    m_MaskedSourceNames = EnumHelper.MaskOutEnumNames<EImageSource>((int)m_SupportedSources);
                
                var index = EnumHelper.GetMaskedIndexFromEnumValue<EImageSource>(m_Source.intValue, (int)m_SupportedSources);
                index = EditorGUILayout.Popup("Source", index, m_MaskedSourceNames);

                if (check.changed)
                    m_Source.intValue = EnumHelper.GetEnumValueFromMaskedIndex<EImageSource>(index, (int)m_SupportedSources);
            }

            var inputType = (EImageSource)m_Source.intValue;
            if ((EImageSource)m_Source.intValue == EImageSource.TaggedCamera )
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CameraTag, new GUIContent("Tag"));
                --EditorGUI.indentLevel;
            }

            if (inputType != EImageSource.RenderTexture)
            {
                m_ResSelector.OnGUI(((ImageInputSettings)target).maxSupportedSize, m_RenderSize);

                if (m_RenderSize.intValue > (int)EImageDimension.Window)
                {
                    EditorGUILayout.PropertyField(m_RenderAspect, new GUIContent("Aspect Ratio"));
                }

//                if(inputType == EImageSource.ActiveCameras)
//                {
//                    AddProperty(m_IncludeUI, () => EditorGUILayout.PropertyField(m_IncludeUI, new GUIContent("Include UI")));
//                }
            }

            //AddProperty(m_Transparency, () => EditorGUILayout.PropertyField(m_Transparency, new GUIContent("Capture alpha"))); // TODO Do same this as wih Include UI

            if (Verbose.enabled)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(m_FlipFinalOutput, new GUIContent("Flip output"));
                }
            }

            //serializedObject.ApplyModifiedProperties();
        }
        
        protected virtual void InitializeTarget(SerializedProperty prop)
        {
            if (target == null)
            {
                string[] path = prop.propertyPath.Split('.');
                target = prop.serializedObject.targetObject;
                foreach (string pathNode in path)
                {
                    target = target.GetType().GetField(pathNode).GetValue(target);
                }
            }
        }
        

        public /*override*/ void CaptureOptionsGUI() // TODO How to?
        {
            //serializedObject.Update();
            
            ++EditorGUI.indentLevel;
            
            var inputType = (EImageSource)m_Source.intValue;
            if (inputType == EImageSource.ActiveCameras)
            {
                EditorGUILayout.PropertyField(m_IncludeUI, new GUIContent("Include UI"));
            }
            
            EditorGUILayout.PropertyField(m_Transparency, new GUIContent("Include alpha"));
            
            --EditorGUI.indentLevel;
            
            //serializedObject.ApplyModifiedProperties();
        }
    }
}
