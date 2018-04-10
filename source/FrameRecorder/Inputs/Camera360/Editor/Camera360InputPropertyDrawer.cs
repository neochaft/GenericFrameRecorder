using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Recorder.Input;

namespace UnityEditor.Recorder.Input
{
    [CustomPropertyDrawer(typeof(Camera360InputSettings))]
    public class Camera360InputPropertyDrawer : InputPropertyDrawer<Camera360InputSettings>
    {
        static EImageSource m_SupportedSources = EImageSource.MainCamera | EImageSource.TaggedCamera;
        string[] m_MaskedSourceNames;

        SerializedProperty m_Source;
        SerializedProperty m_CameraTag;
        SerializedProperty m_FlipFinalOutput;
        SerializedProperty m_StereoSeparation;
        SerializedProperty m_CubeMapSz;
        SerializedProperty m_OutputWidth;
        SerializedProperty m_OutputHeight;
        SerializedProperty m_RenderStereo;

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);
            
            m_Source = property.FindPropertyRelative("source");
            m_CameraTag = property.FindPropertyRelative("cameraTag");

            m_StereoSeparation = property.FindPropertyRelative("stereoSeparation");
            m_FlipFinalOutput = property.FindPropertyRelative("flipFinalOutput");
            m_CubeMapSz = property.FindPropertyRelative("mapSize");
            m_OutputWidth = property.FindPropertyRelative("outputWidth");
            m_OutputHeight = property.FindPropertyRelative("outputHeight");
            m_RenderStereo = property.FindPropertyRelative("renderStereo");
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

            EditorGUILayout.PropertyField(m_OutputWidth, new GUIContent("Output width"));EditorGUILayout.PropertyField(m_OutputHeight, new GUIContent("Output height"));
            

            EditorGUILayout.PropertyField(m_CubeMapSz, new GUIContent("Cube map width"));
            
            EditorGUILayout.PropertyField(m_RenderStereo, new GUIContent("Render in Stereo"));

            ++EditorGUI.indentLevel;
            using (new EditorGUI.DisabledScope(!m_RenderStereo.boolValue))
            {
                EditorGUILayout.PropertyField(m_StereoSeparation, new GUIContent("Stereo Separation"));
            }
            --EditorGUI.indentLevel;

            if (Verbose.enabled)
                EditorGUILayout.LabelField("Flip output", m_FlipFinalOutput.boolValue.ToString());
        }
    }
}