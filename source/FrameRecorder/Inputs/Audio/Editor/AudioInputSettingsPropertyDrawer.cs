using UnityEngine;
using UnityEditor;

namespace Recorder.Input
{
    [CustomPropertyDrawer(typeof(AudioInputSettings))]
    public class AudioInputSettingsPropertyDrawer : InputPropertyDrawer<AudioInputSettings>
    {
        SerializedProperty m_PreserveAudio;
#if RECORD_AUDIO_MIXERS
        SerializedProperty m_AudioMixerGroups;
        ReorderableList    m_AudioMixerGroupsList;
#endif

	    protected override void Initialize(SerializedProperty property)
        {
            m_PreserveAudio = property.FindPropertyRelative("preserveAudio");

#if RECORD_AUDIO_MIXERS
	    m_AudioMixerGroups = serializedObject.FindProperty<AudioInputSettings>(x => x.m_AudioMixerGroups);
	    m_AudioMixerGroupsList = new ReorderableList(serializedObject, m_AudioMixerGroups, true, true, true, true);
	    m_AudioMixerGroupsList.drawElementCallback =
		(Rect rect, int index, bool isActive, bool isFocused) =>
		{
		    var element = m_AudioMixerGroupsList.serializedProperty.GetArrayElementAtIndex(index);
		    rect.y += 2;
		    EditorGUI.PropertyField(
			new Rect(rect.x - 25,                   rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight),
			element.FindPropertyRelative("m_MixerGroup"), GUIContent.none);
		    EditorGUI.PropertyField(
			new Rect(rect.x + rect.width - 85, rect.y, 20,              EditorGUIUtility.singleLineHeight),
			element.FindPropertyRelative("m_Isolate"),    GUIContent.none);
		    EditorGUI.LabelField(
			new Rect(rect.x + rect.width - 65, rect.y, 60,              EditorGUIUtility.singleLineHeight),
			new GUIContent ("Isolate", "Isolate group from mix"));
		};

	    m_AudioMixerGroupsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Audio Mixer Groups");
            };
#endif
        }

	    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	    {
		    Initialize(property);
		    
            EditorGUILayout.PropertyField(m_PreserveAudio, new GUIContent("Capture audio"));

#if RECORD_AUDIO_MIXERS
            if (m_AudioMixerGroups != null)
            {
                m_AudioMixerGroupsList.DoLayoutList();
            }
#endif
        }
    }
}