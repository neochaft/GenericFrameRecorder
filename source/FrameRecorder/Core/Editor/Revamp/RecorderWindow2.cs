using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Recorder.Input;
using Random = UnityEngine.Random;

namespace UnityEditor.Recorder
{
    public partial class RecorderWindow2 : EditorWindow, ISerializationCallbackReceiver
    {
        [MenuItem("Tools/Yolo Record !!")]
        public static void ShowRecorderWindow2()
        {
            GetWindow(typeof(RecorderWindow2), false, "Recorder");
        }

        static Color RandomColor(float alpha = 1.0f)
        {
            return new Color(Random.value, Random.value, Random.value, alpha);
        }

        VisualElement m_Recordings;
        VisualElement m_SettingsPanel;
        VisualElement m_RecordingsPanel;
        
        RecorderEditor m_RecorderEditor;
        VisualElement m_RecorderSettingPanel;
        Button m_StartRecordButton;
        PanelSplitter m_PanelSplitter;
        VisualElement m_AddNewRecord;
        
        VisualElement m_RecordModeOptionsPanel;
        VisualElement m_FrameRateOptionsPanel;

        VisualElement m_StatusBar;

        [SerializeField] GlobalSettings m_GlobalSettings;
        [SerializeField] RecordersList m_RecordersList;
        
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

        GlobalSettingsEditor m_GlobalSettingsEditor;
        
        
        static T LoadSettings<T>(string filename) where T : ScriptableObject
        {
            T asset = null;
            
            var candidates = AssetDatabase.FindAssets("t:" + filename);
            if (candidates.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(candidates[0]);
                asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            
            if(asset == null)
            {
                asset = CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, "Assets/" + filename + ".asset");
                AssetDatabase.Refresh();
            }

            return asset;
        }
        
        public void OnEnable()
        {           
            Random.InitState(1337 + 42);

            var root = this.GetRootVisualContainer();

            const int padding = 10;

            var mainControls = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flexDirection = FlexDirection.Row,
                    minHeight = 110.0f 
                }
            };
            root.Add(mainControls);
            
            var controlLeftPane = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
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
                    backgroundColor = RandomColor(),
                    flex = 0.5f,
                    flexDirection = FlexDirection.Column,
                }
            };
            
            mainControls.Add(controlLeftPane);
            mainControls.Add(controlRightPane);

            var recordIcon = new Image
            {
                image = Resources.Load<Texture2D>("recorder_icon"),

                style =
                {
                    backgroundColor = RandomColor(),
                    width = 80.0f,
                    height = 80.0f,
                }
            };

            controlLeftPane.Add(recordIcon);


            var leftButtonsStack = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    paddingTop = padding,
                    paddingBottom = padding,
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };

            m_StartRecordButton = new Button(OnRecordButtonClick);
            UpdateRecordButtonText();

            leftButtonsStack.Add(m_StartRecordButton);

            m_GlobalSettings = GlobalSettings.instance; 
            
            m_GlobalSettingsEditor = (GlobalSettingsEditor) Editor.CreateEditor(m_GlobalSettings);
                
            m_RecordModeOptionsPanel = new IMGUIContainer(() =>
            {
                if (m_GlobalSettingsEditor.RecordModeGUI())
                    EditorUtility.SetDirty(m_GlobalSettingsEditor);
            })
            {
                style = {flex = 1.0f,}
            };

            leftButtonsStack.Add(m_RecordModeOptionsPanel);

            controlLeftPane.Add(leftButtonsStack);
            
            m_FrameRateOptionsPanel = new IMGUIContainer(() =>
            {
                if (m_GlobalSettingsEditor.FrameRateGUI())
                    EditorUtility.SetDirty(m_GlobalSettingsEditor);
            })
            {
                style = {flex = 1.0f,}
            };
            
            controlRightPane.Add(m_FrameRateOptionsPanel);

            m_SettingsPanel = new ScrollView
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flex = 1.0f,
                }
            };
            
            m_SettingsPanel.contentContainer.style.positionLeft = 0;
            m_SettingsPanel.contentContainer.style.positionRight = 0;

            var recordingAndParameters = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flex = 1.0f,
                    alignSelf = Align.Stretch,
                    flexDirection = FlexDirection.Row,

                }
            };
            
            m_RecordingsPanel = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    width = m_RecordingsPanelWidth,
                    minWidth = 150.0f,
                    maxWidth = 300.0f
                }
            };

            m_PanelSplitter = new PanelSplitter(m_RecordingsPanel);
            
            recordingAndParameters.Add(m_RecordingsPanel);
            recordingAndParameters.Add(m_PanelSplitter.uiElement);
            recordingAndParameters.Add(m_SettingsPanel);

            root.Add(recordingAndParameters);

            var recordingControl = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(0.8f),
                    height = 20,
                }
            };

            m_AddNewRecord = new Label("+ Add New Recordings");
            
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
            
            recordingControl.Add(m_AddNewRecord);
            
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

            m_RecordingsPanel.Add(recordingControl);
            m_RecordingsPanel.Add(m_Recordings);

            var parametersControl = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(0.8f),
                    flex = 1.0f,
                    minWidth = 300.0f,
                }
            };
            
            // TODO UIElements
            m_RecorderSettingPanel = new IMGUIContainer(OnRecorderSettingsGUI)
            {
                style = { flex = 1.0f }
            };

            m_StatusBar = new IMGUIContainer(UpdateRecordingProgressGUI)
            {
                style = { height = EditorGUIUtility.singleLineHeight }
            };
            
            root.Add(m_StatusBar);
            
            parametersControl.Add(m_RecorderSettingPanel);
            
            m_SettingsPanel.Add(parametersControl);            
            
            // Load recorders
            if (m_RecordersList == null)
                m_RecordersList = LoadSettings<RecordersList>("RecordersList");

            foreach (var recorderSettings in m_RecordersList.recorders)
            {
                var info = RecordersInventory.GetRecorderInfo(recorderSettings.recorderType);
                m_Recordings.Add(new RecorderItem(recorderSettings, info.iconName, OnRecordMouseUp));
            }
            
            SelectRecorder(m_Recordings.Children().ElementAt(m_SelectedRecorderItemIndex));
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

            if (m_State != State.Idle)
            {
                m_StartRecordButton.SetEnabled(EditorApplication.isPlaying && Time.frameCount - m_FrameCount > 5.0f);
            }
            else
            {
                m_StartRecordButton.SetEnabled(!EditorApplication.isPlaying);
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
            m_StartRecordButton.text = m_State == State.Recording ? "Stop Recording" : "Start Recording";
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
                var rec = (RecorderSettings)m_RecorderEditor.target;

                //using (new EditorGUI.DisabledScope(ShouldDisableRecordSettings()))
                {
                    EditorGUILayout.LabelField("Recording Type", rec.GetType().Name);

                    m_RecorderEditor.OnInspectorGUI();

                    if (GUI.changed)
                        m_RecorderSettingPanel.Dirty(ChangeType.Layout);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Nothing selected");
            }
        }

        void OnAddNewRecorder(RecorderInfo info)
        {
            var recorderName = ObjectNames.GetUniqueName(m_Recordings.Children().Select(r => ((RecorderItem)r).settings.name).ToArray(),
                ObjectNames.NicifyVariableName(info.displayName));
            m_Recordings.Add(new RecorderItem(m_RecordersList, info.recorderType, recorderName, info.iconName, OnRecordMouseUp));
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
                    contextMenu.AddItem(new GUIContent("Duplicate"), false, data => { }, recorder);
                    contextMenu.AddItem(new GUIContent("Delete"), false,
                        data =>
                        {
                            var item = (RecorderItem) data;
                            var s = item.settings;
                            m_RecordersList.Remove(s);

                            UnityHelpers.Destroy(s, true);
                            UnityHelpers.Destroy(item.editor, true);

                            m_Recordings.Remove(item);

                            AssetDatabase.SaveAssets();

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
                    r.selected = true;
                    m_SelectedRecorderItemIndex = i;
                }
                else
                {
                    r.selected = false;
                    ++i;
                }
            }

            if (m_RecorderEditor != null)
            {
                m_RecorderSettingPanel.Dirty(ChangeType.Layout);
            }
        }

        void UpdateRecordingProgressGUI()
        {
            if (m_State == State.Idle)
            {
                EditorGUILayout.LabelField(new GUIContent("Click START RECORDING button to trigger YOLO mode"));
                return;
            }

            if (m_State == State.WaitingForPlayModeToStartRecording)
            {
                EditorGUILayout.LabelField(new GUIContent("Waiting for playmode to start..."));
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
                    var label = string.Format("{0} Frames processed", session.frameIndex);
                    EditorGUI.ProgressBar(progressBarRect, 0, label);

                    break;
                }
                case RecordMode.SingleFrame:
                case RecordMode.FrameInterval:
                {
                    var label = session.frameIndex < settings.startFrame
                        ? string.Format("Skipping first {0} frames...", settings.startFrame - 1)
                        : string.Format("{0} Frames processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (session.frameIndex + 1) / (float) (settings.endFrame + 1), label);
                    break;
                }
                case RecordMode.TimeInterval:
                {
                    var label = session.m_CurrentFrameStartTS < settings.startTime
                        ? string.Format("Skipping first {0} seconds...", settings.startTime)
                        : string.Format("{0} Frames processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (float) session.m_CurrentFrameStartTS / (settings.endTime.Equals(0.0f) ? 0.0001f : settings.endTime), label);
                    
                    break;
                }
            }
            
//            var recordingSessions = SceneHook.GetCurrentRecordingSessions();
//
//            var session = recordingSessions.FirstOrDefault();
//
//            if (session == null)
//                return;
//                    
//            var progressBarRect = EditorGUILayout.GetControlRect();
//            
//            var settings = session.settings;
//
//            switch (settings.recordMode)
//            {
//                case RecordMode.Manual:
//                {
//                    var label = string.Format("{0} Frames recorded", session.m_Recorder.recordedFramesCount);
//                    EditorGUI.ProgressBar(progressBarRect, 0, label);
//
//                    break;
//                }
//                case RecordMode.SingleFrame:
//                case RecordMode.FrameInterval:
//                {
//                    var label = session.frameIndex < settings.startFrame
//                        ? string.Format("Skipping first {0} frames...", settings.startFrame - 1)
//                        : string.Format("{0} Frames recorded", session.m_Recorder.recordedFramesCount);
//                    EditorGUI.ProgressBar(progressBarRect, (session.frameIndex + 1) / (float) (settings.endFrame + 1), label);
//                    break;
//                }
//                case RecordMode.TimeInterval:
//                {
//                    var label = session.m_CurrentFrameStartTS < settings.startTime
//                        ? string.Format("Skipping first {0} seconds...", settings.startTime)
//                        : string.Format("{0} Frames recorded", session.m_Recorder.recordedFramesCount);
//                    EditorGUI.ProgressBar(progressBarRect, (float) session.m_CurrentFrameStartTS / (settings.endTime.Equals(0.0f) ? 0.0001f : settings.endTime), label);
//                    
//                    break;
//                }
//            }
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
    
    static class UiElementsHelper
    {
        public static VisualElement FieldWithLabel(string label, VisualElement field, float indent = 0.0f)
        {
            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row } 
            };

            var labelElement = new Label(label);
            labelElement.style.paddingLeft = indent;
            labelElement.style.marginRight = 0.0f;
            labelElement.style.marginLeft = 4.0f;
            labelElement.style.width = 150.0f;
            
            container.Add(labelElement);
            
            field.style.flex = 1.0f;
            field.style.minWidth = 55.0f;
            field.style.marginLeft = 0.0f;
            field.style.marginRight = 4.0f;
            
            container.Add(field);

            return container;
        }
    }
}
