using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityObject = UnityEngine.Object;

namespace Recorder.Timeline
{
    [CustomEditor(typeof(RecorderClip), true)]
    public class RecorderClipEditor : Editor
    {
        RecorderEditor m_Editor;
        TimelineAsset m_Timeline;
        RecorderSelector m_RecorderSelector;

        public void OnEnable()
        {
            m_RecorderSelector = null;
        }

        public override void OnInspectorGUI()
        {
            try
            {
                if (target == null)
                    return;

                // Bug? work arround: on Stop play, Enable is not called.
                if (m_Editor != null && m_Editor.target == null)
                {
                    UnityHelpers.Destroy(m_Editor);
                    m_Editor = null;
                    m_RecorderSelector = null;
                }

                if (m_RecorderSelector == null)
                {
                    m_RecorderSelector = new RecorderSelector();
                    m_RecorderSelector.OnSelectionChanged += OnRecorderSelected;
                    m_RecorderSelector.Init(((RecorderClip) target).m_Settings);
                }

                m_RecorderSelector.OnGui();

                if (m_Editor != null)
                {
                    m_Timeline = FindTimelineAsset();

                    PushTimelineIntoRecorder();

                    using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                    {
                        EditorGUILayout.Separator();

                        m_Editor.OnInspectorGUI();

                        EditorGUILayout.Separator();

                        PushRecorderIntoTimeline();

                        serializedObject.Update();
                    }
                }
            }
            catch (ExitGUIException)
            {
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox("An exception was raised while editing the settings. This can be indicative of corrupted settings.", MessageType.Warning);

                if (GUILayout.Button("Reset settings to default"))
                    ResetSettings();

                Debug.LogException(ex);
            }
        }

        void ResetSettings()
        {
            UnityHelpers.Destroy(m_Editor);
            m_Editor = null;
            m_RecorderSelector = null;
            UnityHelpers.Destroy(((RecorderClip) target).m_Settings, true);
        }

        void OnRecorderSelected(Type selectedRecorder)
        {
            var clip = (RecorderClip)target;

            if (m_Editor != null)
            {
                UnityHelpers.Destroy(m_Editor);
                m_Editor = null;
            }

            if (selectedRecorder == null)
                return;

            if (clip.m_Settings != null && RecordersInventory.GetRecorderInfo(selectedRecorder).settingsType != clip.m_Settings.GetType())
            {
                UnityHelpers.Destroy(clip.m_Settings, true);
                clip.m_Settings = null;
            }

            if (clip.m_Settings == null)
            {
                clip.m_Settings = RecordersInventory.CreateDefaultRecorderSettings(selectedRecorder);
                
                AssetDatabase.AddObjectToAsset(clip.m_Settings, clip);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            m_Editor = (RecorderEditor) CreateEditorWithContext(new UnityObject[] { clip.m_Settings}, SceneHook.GetRecorderBindings());
            AssetDatabase.Refresh();
        }

        TimelineAsset FindTimelineAsset()
        {
            if (!AssetDatabase.Contains(target))
                return null;

            var path = AssetDatabase.GetAssetPath(target);
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objs)
            {
                if (obj != null && AssetDatabase.IsMainAsset(obj))
                    return obj as TimelineAsset;
            }
            return null;
        }

        void PushTimelineIntoRecorder()
        {
            if (m_Timeline == null)
                return;

            var settings = m_Editor.target as RecorderSettings;

            // Time
            settings.frameRate = m_Timeline.editorSettings.fps;
        }

        void PushRecorderIntoTimeline()
        {
            if (m_Timeline == null)
                return;

            var settings = m_Editor.target as RecorderSettings;

            // Time
            m_Timeline.editorSettings.fps = settings.frameRate;
        }
    }
}