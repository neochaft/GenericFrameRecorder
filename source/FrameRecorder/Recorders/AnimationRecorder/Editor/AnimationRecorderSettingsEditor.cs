
namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(AnimationRecorderSettings))]
    public class AnimationRecorderSettingsEditor: RecorderEditor
    {
        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.LabelField("Format", "Animation Clip");
        }   
    }
}