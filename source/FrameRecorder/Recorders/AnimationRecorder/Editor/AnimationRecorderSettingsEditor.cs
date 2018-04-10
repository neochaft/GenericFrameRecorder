using UnityEditor.Experimental.Recorder;
using UnityEditor.Recorder;

namespace UnityEditor.Experimental.FrameRecorder
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