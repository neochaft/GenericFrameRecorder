
using UnityEditor;

namespace Recorder
{
    [CustomEditor(typeof(AnimationRecorderSettings))]
    class AnimationRecorderSettingsEditor: RecorderEditor
    {
        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.LabelField("Format", "Animation Clip");
        }   
    }
}