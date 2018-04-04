using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Recorder.Input;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    public class RecorderWindow2 : EditorWindow, ISerializationCallbackReceiver
    {
        [MenuItem("Tools/New Recorder")]
        public static void ShowRecorderWindow2()
        {
            GetWindow(typeof(RecorderWindow2), false, "Recorder");
        }

        VisualElement m_Recordings;
        VisualElement m_SettingsPanel;
        VisualElement m_RecordingsPanel;
        
        RecorderEditor m_RecorderEditor;
        VisualElement m_RecorderSettingPanel;
        Button m_RecordButton;
        PanelSplitter m_PanelSplitter;
        VisualElement m_AddNewRecord;
        
        VisualElement m_RecordModeOptionsPanel;
        VisualElement m_FrameRateOptionsPanel;
        
        [SerializeField] float m_RecordingsPanelWidth = 200.0f;
        [SerializeField] int m_SelectedRecorderItemIndex = 0;
        
        enum State
        {
            Idle,
            WaitingForPlayModeToStartRecording,
            Recording
        }
        
        State m_State = State.Idle;
        int m_FrameCount = 0;

        RecorderViewPrefsEditor m_RecorderViewPrefsEditor;
               
        public void OnEnable()
        {             
            var root = this.GetRootVisualContainer();

            root.style.flexDirection = FlexDirection.Column; 
            
            root.AddStyleSheetPath("recorder");
            root.AddStyleSheetPath(EditorGUIUtility.isProSkin ? "recorder_darkSkin" : "recorder_lightSkin");

            var mainControls = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    minHeight = 110.0f 
                }
            };
            root.Add(mainControls);
            
            var controlLeftPane = new VisualElement
            {
                style =
                {
                    flex = 0.5f,
                    minWidth = 350.0f,
                    maxWidth = 450.0f,
                    flexDirection = FlexDirection.Row,
                }
            };

            var controlRightPane = new VisualElement
            {
                style =
                {
                    flex = 0.5f,
                    flexDirection = FlexDirection.Column,
                }
            };
            
            mainControls.Add(controlLeftPane);
            mainControls.Add(controlRightPane);
            
            controlLeftPane.AddToClassList("StandardPanel");
            controlRightPane.AddToClassList("StandardPanel");

            var recordIcon = new Image
            {
                name = "recorderIcon",
                image = Resources.Load<Texture2D>("recorder_icon")
            };

            controlLeftPane.Add(recordIcon);


            var leftButtonsStack = new VisualElement
            {
                style =
                {
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };

            m_RecordButton = new Button(OnRecordButtonClick)
            {
                name = "recordButton"
            };
            
            UpdateRecordButtonText();

            leftButtonsStack.Add(m_RecordButton); 
                
            m_RecordModeOptionsPanel = new IMGUIContainer(() =>
            {
                if (m_RecorderViewPrefsEditor.RecordModeGUI())
                    EditorUtility.SetDirty(RecorderViewPrefs.instance);
            })
            {
                style = { flex = 1.0f }
            };

            leftButtonsStack.Add(m_RecordModeOptionsPanel);

            controlLeftPane.Add(leftButtonsStack);
            
            m_FrameRateOptionsPanel = new IMGUIContainer(() =>
            {
                if (m_RecorderViewPrefsEditor.FrameRateGUI())
                    EditorUtility.SetDirty(RecorderViewPrefs.instance);
            })
            {
                style = { flex = 1.0f }
            };
            
            controlRightPane.Add(m_FrameRateOptionsPanel);

            m_SettingsPanel = new ScrollView
            {
                style = { flex = 1.0f }
            };
            
            m_SettingsPanel.contentContainer.style.positionLeft = 0;
            m_SettingsPanel.contentContainer.style.positionRight = 0;

            var recordingAndParameters = new VisualElement
            {
                style =
                {
                    flex = 1.0f,
                    alignSelf = Align.Stretch,
                    flexDirection = FlexDirection.Row,

                }
            };
            
            m_RecordingsPanel = new VisualContainer
            {
                name = "recordingsPanel",
                style =
                {
                    width = m_RecordingsPanelWidth,
                    minWidth = 150.0f,
                    maxWidth = 500.0f
                }
            };
            
            m_RecordingsPanel.AddToClassList("StandardPanel");

            m_PanelSplitter = new PanelSplitter(m_RecordingsPanel);
            
            recordingAndParameters.Add(m_RecordingsPanel);
            recordingAndParameters.Add(m_PanelSplitter);
            recordingAndParameters.Add(m_SettingsPanel);
            
            m_SettingsPanel.AddToClassList("StandardPanel");

            root.Add(recordingAndParameters);

            var stuff = new VisualContainer
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            
            m_AddNewRecord = new Label("+ Add New Recordings")
            {
                name = "addRecordingsButton",
                
            };
            
            stuff.Add(m_AddNewRecord);
            stuff.Add(new IMGUIContainer(OnRecorderListSettingPresetGUI));
            
            m_AddNewRecord.RegisterCallback<MouseUpEvent>(evt =>
            {               
                var newRecordMenu = new GenericMenu();

                var recorderList = RecordersInventory.ListRecorders();
                
                foreach (var info in recorderList)
                {
                    if (ShouldDisableRecordSettings())
                        newRecordMenu.AddDisabledItem(new GUIContent(info.displayName));
                    else
                        newRecordMenu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
                }
                
                newRecordMenu.ShowAsContext();
            });
            
            
            m_Recordings = new ScrollView
            {
                
                style =
                {
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };

            m_Recordings.contentContainer.style.positionLeft = 0;
            m_Recordings.contentContainer.style.positionRight = 0;

            m_RecordingsPanel.Add(stuff);
            m_RecordingsPanel.Add(m_Recordings);

            var parametersControl = new VisualElement
            {
                style =
                {
                    flex = 1.0f,
                    minWidth = 300.0f,
                }
            };
            
            // TODO UIElements
            m_RecorderSettingPanel = new IMGUIContainer(OnRecorderSettingsGUI)
            {
                name = "recorderSettings",
                style = { flex = 1.0f }
            };

            var statusBar = new VisualContainer
            {
                name = "statusBar"
            };
            // TODO UIElements
            statusBar.Add(new IMGUIContainer(UpdateRecordingProgressGUI));
            
            root.Add(statusBar);
            
            parametersControl.Add(m_RecorderSettingPanel);
            
            m_SettingsPanel.Add(parametersControl);

            ReloadRecordings();
        }

        void ReloadRecordings()
        {
            m_Recordings.Clear();
            
            foreach (var recorderSettings in RecorderViewPrefs.instance.recorders)
            {
                var info = RecordersInventory.GetRecorderInfo(recorderSettings.recorderType);
                m_Recordings.Add(new RecorderItem(recorderSettings, info.iconName, OnRecordMouseUp));
            }

            if (m_Recordings.Children().Any())
            {
                SelectRecorder(m_Recordings.Children().ElementAt(m_SelectedRecorderItemIndex));
            }
            
            if (m_RecorderViewPrefsEditor != null)
                DestroyImmediate(m_RecorderViewPrefsEditor);
            
            m_RecorderViewPrefsEditor = (RecorderViewPrefsEditor) Editor.CreateEditor(RecorderViewPrefs.instance);
            
            Repaint();
        }

        bool ShouldDisableRecordSettings()
        {
            return m_State != State.Idle || EditorApplication.isPlaying;
        }

        void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (m_State == State.WaitingForPlayModeToStartRecording)
                {
                    DelayedStartRecording();
                }
            }
            else
            {
                if (m_State == State.Recording)
                    m_State = State.Idle;
            }
            
            // TODO Use events instead
            var enable = !ShouldDisableRecordSettings();
            m_AddNewRecord.SetEnabled(enable);
            m_RecorderSettingPanel.SetEnabled(enable);
            m_RecordModeOptionsPanel.SetEnabled(enable);
            m_FrameRateOptionsPanel.SetEnabled(enable);

            if (HaveActiveRecordings())
            {
                if (m_State != State.Idle)
                {
                    m_RecordButton.SetEnabled(EditorApplication.isPlaying && Time.frameCount - m_FrameCount > 5.0f);
                }
                else
                {
                    m_RecordButton.SetEnabled(!EditorApplication.isPlaying);
                }
            }
            else
            {
                m_RecordButton.SetEnabled(false);
            }

            UpdateRecordButtonText();

            if (m_State == State.Recording)
            {
                Repaint();
            }
        }
        
        void DelayedStartRecording()
        {
            StartRecording(true);
        }

        void StartRecording(bool autoExitPlayMode)
        {         
            var sessions = new List<RecordingSession>();
            
            foreach (var visualElement in m_Recordings.Children())
            {
                var recorderItem = (RecorderItem) visualElement;
                var settings = recorderItem.settings;

                if (!settings.enabled)
                {
                    if (Verbose.enabled)
                        Debug.Log("Ignoring disabled recorder '" + settings.name + "'");
                    
                    continue;
                }

                var session = SceneHook.CreateRecorderSession(settings, autoExitPlayMode);
                
                sessions.Add(session);
            }
            
            var success = sessions.All(s => s.SessionCreated() && s.BeginRecording());

            if (success)
            {
                m_State = State.Recording;
                m_FrameCount = 0;
            }
            else
            {
                StopRecording();
            }
        }

        void OnRecordButtonClick()
        {
            switch (m_State)
            {
                case State.Idle:
                {             
                    m_State = State.WaitingForPlayModeToStartRecording;
                    GameViewSize.DisableMaxOnPlay();
                    EditorApplication.isPlaying = true;
                    m_FrameCount = Time.frameCount;
                    
                    break;
                }

                case State.WaitingForPlayModeToStartRecording:
                case State.Recording:
                {   
                    StopRecording();

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            UpdateRecordButtonText();
        }

        void UpdateRecordButtonText()
        {
            m_RecordButton.text = m_State == State.Recording ? "STOP RECORDING" : "START RECORDING";
        }
              
        void StopRecording()
        {
            var recorderGO = SceneHook.GetRecorderHost();
            if (recorderGO != null)
            {
                UnityHelpers.Destroy(recorderGO);
            }
            
            m_State = State.Idle;
            m_FrameCount = 0;
        }

        void OnRecorderSettingsGUI()
        {
            if (m_RecorderEditor != null)
            {

                OnRecorderSettingPresetGUI();

                EditorGUILayout.LabelField("Recording Type", ObjectNames.NicifyVariableName(m_RecorderEditor.target.GetType().Name));

                m_RecorderEditor.OnInspectorGUI();

                if (GUI.changed)
                    m_RecorderSettingPanel.Dirty(ChangeType.Layout);
            }
            else
            {
                EditorGUILayout.LabelField("Nothing selected");
            }
        }
        
        void OnRecorderListSettingPresetGUI()
        {
            if (GUILayout.Button("Load Preset"))
            {
                EditorGUIUtility.ShowObjectPicker<RecorderListPreset>(null, false, "", GUIUtility.GetControlID(FocusType.Passive) + 100);
            }
            
            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
            {
                var candidate = EditorGUIUtility.GetObjectPickerObject() as RecorderListPreset;

                if (candidate == null)
                    return;
                
                RecorderViewPrefs.Load(candidate);
                
                Event.current.Use();

                m_SelectedRecorderItemIndex = 0;
                ReloadRecordings();
            }

            using (new EditorGUI.DisabledScope(RecorderViewPrefs.instance == null))
            {
                if (GUILayout.Button("Save Preset"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Save Preset", RecorderViewPrefs.instance.name + "Preset.asset", "asset", "");

                    if (path.Length != 0)
                    {
                        RecorderListPreset.Save(RecorderViewPrefs.instance, path);
                    }
                }
            }
        }

        class PresetReceiver : PresetSelectorReceiver
        {
            UnityEngine.Object m_Target;
            Preset m_InitialValue;
            EditorWindow m_Window;

            internal void Init(UnityEngine.Object target, EditorWindow window)
            {
                m_Window = window;
                m_Target = target;
                m_InitialValue = new Preset(target);
            }

            public override void OnSelectionChanged(Preset selection)
            {
                if (selection != null)
                {
                    Undo.RecordObject(m_Target, "Apply Preset " + selection.name);
                    
                        selection.ApplyTo(m_Target);
                }
                else
                {
                    Undo.RecordObject(m_Target, "Cancel Preset");
                        m_InitialValue.ApplyTo(m_Target);
                }
                
                m_Window.Repaint();
            }

            public override void OnSelectionClosed(Preset selection)
            {
                OnSelectionChanged(selection);
                DestroyImmediate(this);
            }
        }

        void OnRecorderSettingPresetGUI()
        {
            if (m_RecorderEditor.target != null)
            {
                var rect = EditorGUILayout.GetControlRect();

                if (EditorGUI.DropdownButton(rect, EditorGUIUtility.IconContent("Preset.Context"), FocusType.Passive, new GUIStyle("IconButton")))
                {
                    var presetReceiver = CreateInstance<PresetReceiver>();
                    presetReceiver.Init(m_RecorderEditor.target, this);
                    
                    PresetSelector.ShowSelector(m_RecorderEditor.target, null, true, presetReceiver);
                }
            }
        }

        void OnDestroy()
        {
            RecorderViewPrefs.Release();
            DestroyImmediate(m_RecorderViewPrefsEditor);
        }

        void DuplicateRecording(RecorderSettings candidate)
        {
            var newName = ObjectNames.GetUniqueName(m_Recordings.Children().Select(r => ((RecorderItem)r).settings.name).ToArray(), candidate.name);
            var copy = AssetSettingsHelper.Duplicate(candidate, newName, RecorderViewPrefs.instance);
            RecorderViewPrefs.instance.AddRecorder(copy);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ReloadRecordings();
        }

        void OnAddNewRecorder(RecorderInfo info)
        {
            var recorderName = ObjectNames.GetUniqueName(m_Recordings.Children().Select(r => ((RecorderItem)r).settings.name).ToArray(),
                ObjectNames.NicifyVariableName(info.displayName));
            m_Recordings.Add(new RecorderItem(RecorderViewPrefs.instance, info.recorderType, recorderName, info.iconName, OnRecordMouseUp));
        }

        void OnRecordMouseUp(MouseUpEvent evt)
        {
            if (evt.clickCount != 1)
                return;

            var recorder = (RecorderItem) evt.currentTarget;
            
            if (evt.button == (int) MouseButton.LeftMouse)
            {
              SelectRecorder(recorder);
              evt.StopImmediatePropagation();
            }
            else
            {             
                var contextMenu = new GenericMenu();

                if (ShouldDisableRecordSettings())
                {
                    contextMenu.AddDisabledItem(new GUIContent("Duplicate"));
                    contextMenu.AddDisabledItem(new GUIContent("Delete"));
                }
                else
                {
                    contextMenu.AddItem(new GUIContent("Duplicate"), false,
                        data =>
                        {
                            var item = (RecorderItem) data;
                            var s = item.settings;
                            DuplicateRecording(s);

                            SelectRecorder(m_Recordings.Last());

                        }, recorder);
                    
                    contextMenu.AddItem(new GUIContent("Delete"), false,
                        data =>
                        {
                            var item = (RecorderItem) data;
                            var s = item.settings;
                            RecorderViewPrefs.instance.RemoveRecorder(s);

                            var selected = item.IsItemSelected();

                            UnityHelpers.Destroy(s, true);
                            UnityHelpers.Destroy(item.editor, true);

                            m_Recordings.Remove(item);

                            AssetDatabase.SaveAssets();
                            
                            if (selected)
                                SelectRecorder(m_Recordings.FirstOrDefault());

                        }, recorder);
                }

                contextMenu.ShowAsContext();
            }
        }
       
        void SelectRecorder(VisualElement selected)
        {
            m_RecorderEditor = null;
            m_SelectedRecorderItemIndex = 0;
            
            int i = 0;
            foreach (var recording in m_Recordings)
            {
                var r = (RecorderItem) recording;

                if (recording == selected)
                {
                    m_RecorderEditor = r.editor;
                    r.SetItemSelected(true);
                    m_SelectedRecorderItemIndex = i;
                }
                else
                {
                    r.SetItemSelected(false);
                    ++i;
                }
            }

            if (m_RecorderEditor != null)
            {
                m_RecorderSettingPanel.Dirty(ChangeType.Layout);
            }
        }

        bool HaveActiveRecordings()
        {
            return RecorderViewPrefs.instance.recorders.Any(r => r.enabled);
        }

        void UpdateRecordingProgressGUI()
        {
            if (m_State == State.Idle)
            {
                EditorGUILayout.LabelField(HaveActiveRecordings() ? new GUIContent("Ready to start recording") : new GUIContent("No active recording"));
                return;
            }

            if (m_State == State.WaitingForPlayModeToStartRecording)
            {
                EditorGUILayout.LabelField(new GUIContent("Waiting for Playmode to start..."));
                return;
            }
            
            var recordingSessions = SceneHook.GetCurrentRecordingSessions();

            var session = recordingSessions.FirstOrDefault(); // Hack. We know each session uses the same global settings so take the first one...

            if (session == null)
                return;
                    
            var progressBarRect = EditorGUILayout.GetControlRect();
            
            var settings = session.settings;

            switch (settings.recordMode)
            {
                case RecordMode.Manual:
                {
                    var label = string.Format("{0} Frame(s) processed", session.frameIndex);
                    EditorGUI.ProgressBar(progressBarRect, 0, label);

                    break;
                }
                case RecordMode.SingleFrame:
                case RecordMode.FrameInterval:
                {
                    var label = session.frameIndex < settings.startFrame
                        ? string.Format("Skipping first {0} frame(s)...", settings.startFrame - 1)
                        : string.Format("{0} Frame(s) processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (session.frameIndex + 1) / (float) (settings.endFrame + 1), label);
                    break;
                }
                case RecordMode.TimeInterval:
                {
                    var label = session.m_CurrentFrameStartTS < settings.startTime
                        ? string.Format("Skipping first {0} second(s)...", settings.startTime)
                        : string.Format("{0} Frame(s) processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (float) session.m_CurrentFrameStartTS / (settings.endTime.Equals(0.0f) ? 0.0001f : settings.endTime), label);
                    
                    break;
                }
            }
        }

        public void OnBeforeSerialize()
        {
            m_RecordingsPanelWidth = m_RecordingsPanel.style.width;
        }

        public void OnAfterDeserialize()
        {
            // Nothing
        }
    }
}
