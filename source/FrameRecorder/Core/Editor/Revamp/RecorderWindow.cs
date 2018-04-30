using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.Recorder;
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
    public class RecorderWindow : EditorWindow
    {
        [MenuItem("Tools/Media Recorder")]
        public static void ShowRecorderWindow()
        {
            GetWindow(typeof(RecorderWindow), false, "Recorder");
        }

        class RicorderItemList : VisualListItem<RecorderItem>
        {
        }
        
        RicorderItemList m_RecordingListItem;
        
        VisualElement m_SettingsPanel;
        VisualElement m_RecordersPanel;
        
        Editor m_RecorderEditor;
        VisualElement m_ParametersControl;
        VisualElement m_RecorderSettingPanel;
        Button m_RecordButton;
        PanelSplitter m_PanelSplitter;
        VisualElement m_AddNewRecordPanel;
        
        VisualElement m_RecordModeOptionsPanel;
        VisualElement m_FrameRateOptionsPanel;
        
        RecorderSettingsPrefs m_Prefs;
        
        static List<RecorderInfo> s_BuiltInRecorderInfos;
        
        enum State
        {
            Idle,
            WaitingForPlayModeToStartRecording,
            Recording
        }
        
        State m_State = State.Idle;
        int m_FrameCount = 0;

        RecorderSettingsPrefsEditor m_RecorderSettingsPrefsEditor;

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
                if (m_RecorderSettingsPrefsEditor.RecordModeGUI())
                    OnGlobalSettingsChanged();
            })
            {
                style = { flex = 1.0f }
            };

            leftButtonsStack.Add(m_RecordModeOptionsPanel);

            controlLeftPane.Add(leftButtonsStack);
            
            m_FrameRateOptionsPanel = new IMGUIContainer(() =>
            {
                if (m_RecorderSettingsPrefsEditor.FrameRateGUI())
                    OnGlobalSettingsChanged();
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

            var recordersAndParameters = new VisualElement
            {
                style =
                {
                    flex = 1.0f,
                    alignSelf = Align.Stretch,
                    flexDirection = FlexDirection.Row,
                }
            };
            
            m_RecordersPanel = new VisualElement
            {
                name = "recordersPanel",
                style =
                {
                    width = 200.0f,
                    minWidth = 150.0f,
                    maxWidth = 500.0f
                }
            };
            
            m_RecordersPanel.AddToClassList("StandardPanel");

            m_PanelSplitter = new PanelSplitter(m_RecordersPanel)
            {
                persistenceKey = "RecordingPanelSplitter"
            };
            
            recordersAndParameters.Add(m_RecordersPanel);
            recordersAndParameters.Add(m_PanelSplitter);
            recordersAndParameters.Add(m_SettingsPanel);
            
            m_SettingsPanel.AddToClassList("StandardPanel");

            root.Add(recordersAndParameters);
            
            var addRecordButton = new Label("+ Add New Recorders")
            {
                style = { flex = 1.0f }
            };

            var recorderListPresetButton = new VisualElement
            {
                name = "recorderListPreset",                
            };
            
            recorderListPresetButton.RegisterCallback<MouseUpEvent>(evt => ShowRecorderListMenu());
            
            recorderListPresetButton.Add(new Image
            {
                image = (Texture2D)EditorGUIUtility.Load("Builtin Skins/" + (EditorGUIUtility.isProSkin ? "DarkSkin" : "LightSkin") + "/Images/pane options.png"),
                style =
                {
                    sliceTop = 0,
                    sliceBottom = 0,
                    sliceLeft = 0,
                    sliceRight = 0,
                    paddingTop = 0.0f,
                    paddingLeft = 0.0f,
                    paddingBottom = 0.0f,
                    paddingRight = 0.0f,
                    width = 16.0f,
                    height = 11.0f
                }
            });
            
            addRecordButton.AddToClassList("RecorderListHeader");
            recorderListPresetButton.AddToClassList("RecorderListHeader");
            
            addRecordButton.RegisterCallback<MouseUpEvent>(evt => ShowNewRecorderMenu());            
            
            m_AddNewRecordPanel = new VisualElement
            {
                name = "addRecordersButton",
                style = { flexDirection = FlexDirection.Row }
            };
            
                                    
            m_AddNewRecordPanel.Add(addRecordButton);
            m_AddNewRecordPanel.Add(recorderListPresetButton);
            
            m_RecordingListItem = new RicorderItemList
            {
                persistenceKey = "RecordersList",
                style = { flex = 1.0f }
            };
            
            m_RecordingListItem.OnItemContextMenu += OnRecordContextMenu;
            m_RecordingListItem.OnSelectionChanged += OnRecordSelectionChanged;
            m_RecordingListItem.OnItemRename += item => item.StartRenaming();
            m_RecordingListItem.OnContextMenu += ShowNewRecorderMenu;

            m_RecordersPanel.Add(m_AddNewRecordPanel);
            m_RecordersPanel.Add(m_RecordingListItem);

            m_ParametersControl = new VisualElement
            {
                style =
                {
                    flex = 1.0f,
                    minWidth = 300.0f,
                }
            };
            
            m_ParametersControl.Add(new Button(OnRecorderSettingPresetClicked)
            {
                name = "presetButton",
                style =
                {
                    alignSelf = Align.FlexEnd,
                    backgroundImage = (Texture2D)EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Preset.Context" : "Preset.Context"),
                    backgroundSize = ScaleMode.ScaleToFit,
                    sliceTop = 0,
                    sliceBottom = 0,
                    sliceLeft = 0,
                    sliceRight = 0,
                    paddingTop = 0.0f,
                    paddingLeft = 0.0f,
                    paddingBottom = 0.0f,
                    paddingRight = 0.0f,
                }
            });
            
            m_RecorderSettingPanel = new IMGUIContainer(OnRecorderSettingsGUI)
            {
                name = "recorderSettings",
                style = { flex = 1.0f }
            };

            var statusBar = new VisualElement
            {
                name = "statusBar"
            };

            statusBar.Add(new IMGUIContainer(UpdateRecordingProgressGUI));
            
            root.Add(statusBar);
            
            m_ParametersControl.Add(m_RecorderSettingPanel);
            
            m_SettingsPanel.Add(m_ParametersControl);

            m_Prefs = RecorderSettingsPrefs.instance;
            
            m_RecorderSettingsPrefsEditor = (RecorderSettingsPrefsEditor) Editor.CreateEditor(m_Prefs);
            
            m_RecordingListItem.RegisterCallback<IMGUIEvent>(OnIMGUIEvent);
            m_RecordingListItem.focusIndex = 0;
            
            ReloadRecordings();

            Undo.undoRedoPerformed +=  SaveAndRepaint;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            // TODO More synch feature?
            // TODO Remove auto session cleanup that can causes crashes?
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                if (m_State == State.WaitingForPlayModeToStartRecording)
                {
                    m_State = State.Idle;
                }
                else if (m_State == State.Recording)
                {
                    StopRecording();
                }
            }
        }

        void OnGlobalSettingsChanged()
        {
            if (m_Prefs == null)
                return;
            
            m_Prefs.ApplyGlobalSettingToAllRecorders();

            SaveAndRepaint();
        }

        void SaveAndRepaint()
        {   
            if (m_Prefs != null)
                m_Prefs.Save();
            
            if (m_RecorderEditor != null)
                m_RecorderSettingPanel.Dirty(ChangeType.Layout | ChangeType.Styles);
            
            Repaint();
        }

        void ReloadRecordings()
        {
            if (m_Prefs == null)
                return;
            
            var recorderItems = m_Prefs.recorders.Select(CreateRecorderItem);
            m_RecordingListItem.Reload(recorderItems);
        }
        
        void OnIMGUIEvent(IMGUIEvent evt)
        {
            if (evt.imguiEvent.type == EventType.ValidateCommand)
            { 
                if (m_RecordingListItem.selection == null)
                    return;

                if (evt.imguiEvent.commandName == "Duplicate" ||
                    evt.imguiEvent.commandName == "SoftDelete" || evt.imguiEvent.commandName == "Delete")
                {
                    evt.StopPropagation();
                }
            }
            else if (evt.imguiEvent.type == EventType.ExecuteCommand)
            {
                var item = m_RecordingListItem.selection;
                
                if (item == null)
                    return;

                if (evt.imguiEvent.commandName == "Duplicate")
                {
                    DuplicateRecorder(item);
                    evt.StopPropagation();
                }
                else if (evt.imguiEvent.commandName == "SoftDelete" || evt.imguiEvent.commandName == "Delete")
                {
                    DeleteRecorder(item, true);
                    evt.StopPropagation();
                }
            }
        }

        void ApplyPreset(string presetPath)
        {           
            var candidate = AssetDatabase.LoadAssetAtPath<RecorderListPreset>(presetPath);

            if (candidate == null)
                return;
                
            candidate.AppyTo(m_Prefs);
            ReloadRecordings();
        }

        void ShowNewRecorderMenu()
        {
            if (s_BuiltInRecorderInfos == null)
            {
                s_BuiltInRecorderInfos = new List<RecorderInfo>
                {
                    RecordersInventory.GetRecorderInfo(typeof(AnimationRecorder)),
                    RecordersInventory.GetRecorderInfo(typeof(VideoRecorder)),
                    RecordersInventory.GetRecorderInfo(typeof(ImageRecorder)),
                };
            }

            var newRecordMenu = new GenericMenu();
                
            foreach (var info in s_BuiltInRecorderInfos)
            {
                if (ShouldDisableRecordSettings())
                    newRecordMenu.AddDisabledItem(new GUIContent(info.displayName));
                else
                    newRecordMenu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
            }
                
            newRecordMenu.AddSeparator(string.Empty);
                
            var recorderList = RecordersInventory.recorderInfos.Where(r => !s_BuiltInRecorderInfos.Contains(r));
                
            foreach (var info in recorderList)
            {
                if (ShouldDisableRecordSettings())
                    newRecordMenu.AddDisabledItem(new GUIContent(info.displayName));
                else
                    newRecordMenu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
            }
                
            newRecordMenu.ShowAsContext();
        }

        RecorderItem CreateRecorderItem(RecorderSettings recorderSettings)
        {
            var info = RecordersInventory.GetRecorderInfo(recorderSettings.recorderType);
            return new RecorderItem(m_Prefs, recorderSettings, info.iconName);
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
                {
                    StopRecording();
                }
            }
            
            var enable = !ShouldDisableRecordSettings();
                
            m_AddNewRecordPanel.SetEnabled(enable);
            m_ParametersControl.SetEnabled(enable);
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
            if (Verbose.enabled)
                Debug.Log("Start Recording.");
            
            var sessions = new List<RecordingSession>();
            
            foreach (var recorder in m_Prefs.recorders)
            {
                m_Prefs.ApplyGlobalSetting(recorder);
                
                if (!m_Prefs.IsRecorderEnabled(recorder))
                {
                    if (Verbose.enabled)
                        Debug.Log("Ignoring disabled recorder '" + recorder.name + "'");
                    
                    continue;
                }

                var session = SceneHook.CreateRecorderSession(recorder, autoExitPlayMode);
                
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
            if (Verbose.enabled)
                Debug.Log("Stop Recording.");
            
            var recorderGO = SceneHook.GetRecorderHost();
            if (recorderGO != null)
            {
                UnityHelpers.Destroy(recorderGO);
            }
            
            m_State = State.Idle;
            m_FrameCount = 0;
            
            // Settings might have changed after the session ended
            m_Prefs.Save();
        }

        void OnRecorderSettingsGUI()
        {
            if (m_RecorderEditor != null)
            {
                EditorGUILayout.LabelField("Recorder Type", ObjectNames.NicifyVariableName(m_RecorderEditor.target.GetType().Name));
                
                if (!(m_RecorderEditor is RecorderEditor))
                    EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

                m_RecorderEditor.OnInspectorGUI();

                if (GUI.changed)
                {
                    m_Prefs.Save(); // TODO Might be too much to save everychange but how then?
                    m_RecorderSettingPanel.Dirty(ChangeType.Layout);
                }
            }
            else
            {
                EditorGUILayout.LabelField("No recorder selected");
            }
        }
       
        void ShowRecorderListMenu()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Save Recorder List"), false, () =>
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Preset", "RecorderSettingPreset.asset", "asset", "");

                if (path.Length != 0)
                    RecorderListPreset.SaveAtPath(m_Prefs, path);
            });

            var presets = AssetDatabase.FindAssets("t:" + typeof(RecorderListPreset).Name);

            if (presets.Length > 0)
            {
                foreach (var preset in presets)
                {
                    var path = AssetDatabase.GUIDToAssetPath(preset);
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    menu.AddItem(new GUIContent("Load Recorder List/" + fileName), false, data => { ApplyPreset((string)data); }, path);
                }
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Load Recorder List"));
            }

            var items = m_RecordingListItem.items.ToArray();

            if (items.Length > 0)
            {
                menu.AddItem(new GUIContent("Clear Recorder List"), false, () =>
                {
                    if (EditorUtility.DisplayDialog("Clear Recoder List?", "All recorder will be deleted. Proceed?", "Delete Recorders"))
                    {
                        foreach (var item in items)
                            DeleteRecorder(item, false);
                        
                        DestroyImmediate(m_RecorderEditor);
                        m_RecorderEditor = null;
                        ReloadRecordings();
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Clear Recorder List"));
            }

            menu.ShowAsContext();
        }

        class PresetReceiver : PresetSelectorReceiver
        {
            RecorderSettings m_Target;
            Preset m_InitialValue;
            EditorWindow m_Window;

            internal void Init(RecorderSettings target, EditorWindow window)
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
                
                m_Target.OnAfterDuplicate();
                
                DestroyImmediate(this);
            }
        }

        void OnRecorderSettingPresetClicked()
        {
            if (m_RecorderEditor != null && m_RecorderEditor.target != null)
            {
                var presetReceiver = CreateInstance<PresetReceiver>();
                presetReceiver.Init((RecorderSettings)m_RecorderEditor.target, this);
                
                PresetSelector.ShowSelector(m_RecorderEditor.target, null, true, presetReceiver);
            }
        }

        void OnDestroy()
        {
            if (m_Prefs != null)
            {
                m_Prefs.Save();
                DestroyImmediate(m_Prefs); // TODO Also destroy the RecorderSettings inside the prefs; // TODO Use Dispose
            }

            if (m_RecorderSettingsPrefsEditor != null)
                DestroyImmediate(m_RecorderSettingsPrefsEditor);
            
            if (m_RecorderEditor != null)
                DestroyImmediate(m_RecorderEditor);
        }

        void AddLastAndSelect(RecorderSettings recorder, string desiredName, bool enabled)
        {
            recorder.name = GetUniqueRecorderName(desiredName);
            m_Prefs.AddRecorder(recorder, recorder.name, enabled);

            m_RecordingListItem.AddAndSelect(CreateRecorderItem(recorder));
        }

        void DuplicateRecorder(RecorderItem item)
        {
            var candidate = item.settings;
            var copy = Instantiate(candidate);
            copy.OnAfterDuplicate();
            AddLastAndSelect(copy, m_Prefs.GetRecorderDisplayName(candidate), m_Prefs.IsRecorderEnabled(candidate));
        }

        void DeleteRecorder(RecorderItem item, bool prompt)
        {
            if (!prompt || EditorUtility.DisplayDialog("Delete Recoder?",
                    "Are you sure you want to delete '" + m_Prefs.GetRecorderDisplayName(item.settings) + "' ?", "Delete"))
            {

                var s = item.settings;
                m_Prefs.RemoveRecorder(s);
                UnityHelpers.Destroy(s, true);
                UnityHelpers.Destroy(item.editor, true);
                m_RecordingListItem.Remove(item);
            }
        }

        void OnAddNewRecorder(RecorderInfo info)
        {           
            var recorder = RecordersInventory.CreateDefaultRecorder(info.recorderType);  
            AddLastAndSelect(recorder, ObjectNames.NicifyVariableName(info.displayName), true);
            
            m_RecorderSettingPanel.Dirty(ChangeType.All);
        }

        string GetUniqueRecorderName(string desiredName)
        {
            return ObjectNames.GetUniqueName(m_Prefs.recorders.Select(r => m_Prefs.GetRecorderDisplayName(r)).ToArray(),
                desiredName);
        }

        void OnRecordContextMenu(RecorderItem recorder)
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
                        DuplicateRecorder((RecorderItem) data);
                    }, recorder);
                
                contextMenu.AddItem(new GUIContent("Delete"), false,
                    data =>
                    {
                        DeleteRecorder((RecorderItem) data, true);
                    }, recorder);
            }

            contextMenu.ShowAsContext();
        }
        
        void OnRecordSelectionChanged()
        {        
            m_RecorderEditor = null;
            
            foreach (var r in m_RecordingListItem.items)
            {
                if (m_RecordingListItem.selection == r)
                {
                    if (m_RecorderEditor == null)
                        m_RecorderEditor = r.editor;
                    
                    r.SetItemSelected(true);
                }
                else
                {
                    r.SetItemSelected(false);
                }
            }

            if (m_RecorderEditor != null)
            {
                m_RecorderSettingPanel.Dirty(ChangeType.Layout | ChangeType.Styles);
            }
            
            Repaint();
        }

        bool HaveActiveRecordings()
        {
            return m_Prefs.recorders.Any(r => m_Prefs.IsRecorderEnabled(r));
        }

        void UpdateRecordingProgressGUI()
        {
            if (m_State == State.Idle)
            {
                EditorGUILayout.LabelField(HaveActiveRecordings() ? new GUIContent("Ready to start recording") : new GUIContent("No active recorder"));
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
    }
}
