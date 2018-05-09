using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

namespace Recorder.Timeline
{
    [CustomEditor(typeof(RecorderClip), true)]
    public class RecorderClipEditor : Editor
    {
        RecorderEditor m_Editor;
        //TimelineAsset m_Timeline;
        //RecorderSelector m_RecorderSelector;

        public void OnEnable()
        {
            //m_RecorderSelector = null;
        }

        public override void OnInspectorGUI()
        {
            try
            {
                if (target == null)
                    return;

//                // Bug? work arround: on Stop play, Enable is not called.
//                if (m_Editor != null && m_Editor.target == null)
//                {
//                    UnityHelpers.Destroy(m_Editor);
//                    m_Editor = null;
//                    m_RecorderSelector = null;
//                }

//                if (m_RecorderSelector == null)
//                {
//                    m_RecorderSelector = new RecorderSelector(OnRecorderSelected);
//                    //m_RecorderSelector.Init(((RecorderClip) target).m_Settings);
//                }
//
//                m_RecorderSelector.OnGui();

                if (m_Editor != null)
                {
//                    m_Timeline = FindTimelineAsset();
//
//                    PushTimelineIntoRecorder();

//                    using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
//                    {
//                        EditorGUILayout.Separator();
//
//                        m_Editor.OnInspectorGUI();
//
//                        EditorGUILayout.Separator();
//
//                        PushRecorderIntoTimeline();
//
//                        serializedObject.Update();
//                    }
                }
            }
            catch (ExitGUIException)
            {
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox("An exception was raised while editing the settings. This can be indicative of corrupted settings.", MessageType.Warning);

//                if (GUILayout.Button("Reset settings to default"))
//                    ResetSettings();

                Debug.LogException(ex);
            }
        }

//        void ResetSettings()
//        {
//            UnityHelpers.Destroy(m_Editor);
//            m_Editor = null;
//            m_RecorderSelector = null;
//            UnityHelpers.Destroy((target as RecorderClip).m_Settings, true);
//        }
//
//        public void OnRecorderSelected()
//        {
//            var clip = this.target as RecorderClip;
//
//            if (m_Editor != null)
//            {
//                UnityHelpers.Destroy(m_Editor);
//                m_Editor = null;
//            }
//
//            if (m_RecorderSelector.selectedRecorder == null)
//                return;
//
//            if (clip.m_Settings != null && RecordersInventory.GetRecorderInfo(m_RecorderSelector.selectedRecorder).settingsType != clip.m_Settings.GetType())
//            {
//                UnityHelpers.Destroy(clip.m_Settings, true);
//                clip.m_Settings = null;
//            }
//
//            if(clip.m_Settings == null)
//                clip.m_Settings = RecordersInventory.CreateDefaultRecorderSettings(m_RecorderSelector.selectedRecorder.GetType());
//            m_Editor = CreateEditor(clip.m_Settings) as RecorderEditor;
//            AssetDatabase.Refresh();
//        }
//
//        TimelineAsset FindTimelineAsset()
//        {
//            if (!AssetDatabase.Contains(target))
//                return null;
//
//            var path = AssetDatabase.GetAssetPath(target);
//            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
//
//            foreach (var obj in objs)
//            {
//                if (obj != null && AssetDatabase.IsMainAsset(obj))
//                    return obj as TimelineAsset;
//            }
//            return null;
//        }
//
//        void PushTimelineIntoRecorder()
//        {
//            if (m_Timeline == null)
//                return;
//
//            var settings = (RecorderSettings)m_Editor.target;
//            
//            settings.recordMode = RecordMode.Manual;
//
//            // Time
//            settings.frameRate = m_Timeline.editorSettings.fps;
//        }
//
//        void PushRecorderIntoTimeline()
//        {
//            if (m_Timeline == null)
//                return;
//
//            var settings = (RecorderSettings)m_Editor.target;
//            settings.recordMode = RecordMode.Manual;
//
//            // Time
//            m_Timeline.editorSettings.fps = settings.frameRate;
//        }
    }
}
