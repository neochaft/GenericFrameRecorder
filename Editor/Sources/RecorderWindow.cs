using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Recorder.Input;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    public class RecorderWindow : EditorWindow
    {
        static string s_WindowTitle = "Recorder";
        
        [MenuItem("Recorder/Recorder Window", false, 1)]
        static void ShowRecorderWindow()
        {
            GetWindow(typeof(RecorderWindow), false, s_WindowTitle);
        }
        
        [MenuItem("Recorder/Quick Recording _F10", false, 2)]
        static void QuickRecording()
        {
            var recorderWindow = (RecorderWindow) GetWindow(typeof(RecorderWindow), false, s_WindowTitle, false);

            if (!recorderWindow.IsRecording())
            {
                recorderWindow.StartRecording();
            }
            else
            {
                recorderWindow.StopRecording();
            }
        }

        class RicorderItemList : VisualListItem<RecorderItem>
        {
        }
        
        RicorderItemList m_RecordingListItem;
        
        VisualElement m_SettingsPanel;
        VisualElement m_RecordersPanel;
        
        RecorderItem m_SelectedRecorderItem;
        
        VisualElement m_ParametersControl;
        VisualElement m_RecorderSettingPanel;
        
        Button m_RecordButton;
        Button m_RecordButtonIcon;
        
        PanelSplitter m_PanelSplitter;
        VisualElement m_AddNewRecordPanel;
        
        VisualElement m_RecordModeOptionsPanel;
        VisualElement m_FrameRateOptionsPanel;
        
        RecorderControllerSettings m_ControllerSettings;
        RecorderController m_RecorderController;

        static readonly string s_PrefsFileName = "/../Library/Recorder/recorder.pref";
        static readonly string s_StylesFolder = "Styles/";
        
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
            
            root.AddStyleSheetPath(s_StylesFolder + "recorder");
            root.AddStyleSheetPath(s_StylesFolder + (EditorGUIUtility.isProSkin ? "recorder_darkSkin" : "recorder_lightSkin"));

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

            m_RecordButtonIcon = new Button(OnRecordButtonClick)
            {
                name = "recorderIcon",
                style =
                {
                    backgroundImage = Resources.Load<Texture2D>("recorder_icon"),
                }
            };

            controlLeftPane.Add(m_RecordButtonIcon);


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

            m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + s_PrefsFileName);
            m_RecorderController = new RecorderController(m_ControllerSettings);
            
            m_RecorderSettingsPrefsEditor = (RecorderSettingsPrefsEditor) Editor.CreateEditor(m_ControllerSettings);
            
            m_RecordingListItem.RegisterCallback<IMGUIEvent>(OnIMGUIEvent);
            m_RecordingListItem.focusIndex = 0;
            
            ReloadRecordings();

            Undo.undoRedoPerformed += SaveAndRepaint;
            
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
                    StopRecordingInternal();
                }
            }
        }

        void OnGlobalSettingsChanged()
        {
            if (m_ControllerSettings == null)
                return;
            
            m_ControllerSettings.ApplyGlobalSettingToAllRecorders();

            SaveAndRepaint();
        }

        void SaveAndRepaint()
        {   
            if (m_ControllerSettings != null)
                m_ControllerSettings.Save();
            
            if (m_SelectedRecorderItem != null)
                m_RecorderSettingPanel.Dirty(ChangeType.Layout | ChangeType.Styles);
            
            Repaint();
        }

        void ReloadRecordings()
        {
            if (m_ControllerSettings == null)
                return;
            
            var recorderItems = m_ControllerSettings.recorderSettings.Select(CreateRecorderItem);
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

        public void ApplyPreset(string presetPath)
        {           
            var candidate = AssetDatabase.LoadAssetAtPath<RecorderControllerSettingsPreset>(presetPath);

            if (candidate == null)
                return;
                
            candidate.AppyTo(m_ControllerSettings);
            ReloadRecordings();
        }

        void ShowNewRecorderMenu()
        {
            var newRecordMenu = new GenericMenu();
                
            foreach (var info in RecordersInventory.builtInRecorderInfos)
                AddRecorderInfoToMenu(info, newRecordMenu);

            if (Options.showLegacyRecorders)
            {
                newRecordMenu.AddSeparator(string.Empty);
                
                foreach (var info in RecordersInventory.legacyRecorderInfos)
                    AddRecorderInfoToMenu(info, newRecordMenu);
            }
                
            var recorderList = RecordersInventory.customRecorderInfos.ToList();

            if (recorderList.Any())
            {
                newRecordMenu.AddSeparator(string.Empty);
                
                foreach (var info in recorderList)
                    AddRecorderInfoToMenu(info, newRecordMenu);
            }
            
            newRecordMenu.ShowAsContext();
        }

        void AddRecorderInfoToMenu(RecorderInfo info, GenericMenu menu)
        {
            if (ShouldDisableRecordSettings())
                menu.AddDisabledItem(new GUIContent(info.displayName));
            else
                menu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
        }
        
        RecorderItem CreateRecorderItem(RecorderSettings recorderSettings)
        {
            var info = RecordersInventory.GetRecorderInfo(recorderSettings.GetType());
           
            var hasError = info == null; 
            
            var recorderItem = new RecorderItem(m_ControllerSettings, recorderSettings, hasError ? null : info.iconName);
            
            if (hasError)
                recorderItem.state = RecorderItem.State.HasErrors;

            return recorderItem;
        }

        string CheckRecordersIncompatibility()
        {
            var activeRecorders = m_ControllerSettings.recorderSettings.Where(r => r.enabled).ToArray();

            if (activeRecorders.Length == 0)
                return null;

            var outputPaths = new HashSet<string>();

            foreach (var recorder in activeRecorders)
            {
                var path = recorder.fileNameGenerator.BuildAbsolutePath(null); // Does not detect all conflict or might have false positives
                if (outputPaths.Contains(path))
                    return "Some recorders might try to save into the same output file.";
                
                outputPaths.Add(path);
            }
            
            var ii = m_ControllerSettings.recorderSettings.Where(r => r.enabled).SelectMany(r =>
                r.inputsSettings.Where(i => i is GameViewInputSettings)).ToList();
            
            if (ii.Count >= 2)
                return "Using Game View on multiple recorders can lead to unespected behaviour.";

            return null;
        }

        bool ShouldDisableRecordSettings()
        {
            return IsRecording() || EditorApplication.isPlaying;
        }

        void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (m_State == State.WaitingForPlayModeToStartRecording)
                {
                    StartRecordingInternal();
                }
            }
            else
            {
                if (m_State == State.Recording)
                {
                    StopRecordingInternal();
                }
            }
            
            var enable = !ShouldDisableRecordSettings();
                
            m_AddNewRecordPanel.SetEnabled(enable);
            m_ParametersControl.SetEnabled(enable && m_SelectedRecorderItem != null && m_SelectedRecorderItem.state != RecorderItem.State.HasErrors);
            m_RecordModeOptionsPanel.SetEnabled(enable);
            m_FrameRateOptionsPanel.SetEnabled(enable);

            if (HaveActiveRecordings())
            {
                if (IsRecording())
                {
                    SetRecordButtonsEnabled(EditorApplication.isPlaying && Time.frameCount - m_FrameCount > 5.0f);
                }
                else
                {
                    SetRecordButtonsEnabled(true);
                }
            }
            else
            {
                SetRecordButtonsEnabled(false);
            }

            UpdateRecordButtonText();

            if (m_State == State.Recording)
            {
                Repaint();
            }
        }

        void SetRecordButtonsEnabled(bool enabled)
        {
            m_RecordButton.SetEnabled(enabled);
            m_RecordButtonIcon.SetEnabled(enabled);
        }

        void StartRecordingInternal()
        {        
            if (Options.debugMode)
                Debug.Log("Start Recording.");

            m_RecorderController.verbose = Options.debugMode;
            var success = m_RecorderController.StartRecording();
            
            if (success)
            {
                m_State = State.Recording;
                m_FrameCount = 0;
            }
            else
            {
                StopRecordingInternal();
            }
        }

        public void StartRecording()
        {
            if (m_State == State.Idle)
            {
                m_State = State.WaitingForPlayModeToStartRecording;
                GameViewSize.DisableMaxOnPlay();
                EditorApplication.isPlaying = true;
                m_FrameCount = Time.frameCount;
            }
        }

        public bool IsRecording()
        {
            return m_State != State.Idle;
        }
        
        public void StopRecording()
        {
            if (IsRecording())
                StopRecordingInternal();
        }

        void OnRecordButtonClick()
        {
            switch (m_State)
            {
                case State.Idle:
                {             
                    StartRecording();
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
        
        void StopRecordingInternal()
        {
            if (Options.debugMode)
                Debug.Log("Stop Recording.");
            
            m_RecorderController.StopRecording();
            
            m_State = State.Idle;
            m_FrameCount = 0;
            
            // Settings might have changed after the session ended
            m_ControllerSettings.Save();
        }

        void OnRecorderSettingsGUI()
        {
            if (m_SelectedRecorderItem != null)
            {
                if (m_SelectedRecorderItem.state == RecorderItem.State.HasErrors)
                {
                    EditorGUILayout.LabelField("Missing reference to the recorder."); // TODO Better messaging
                }
                else
                {
                    var editor = m_SelectedRecorderItem.editor;

                    if (editor == null)
                    {
                        EditorGUILayout.LabelField("Error while displaying the recorder inspector"); // TODO Better messaging
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Recorder Type",
                            ObjectNames.NicifyVariableName(editor.target.GetType().Name));

                        if (!(editor is RecorderEditor))
                            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

                        editor.OnInspectorGUI();

                        if (GUI.changed)
                        {
                            m_ControllerSettings.Save(); // TODO Might be too much to save everychange but how then?
                            m_RecorderSettingPanel.Dirty(ChangeType.Layout);
                        }
                    }
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
                    RecorderControllerSettingsPreset.SaveAtPath(m_ControllerSettings, path);
            });

            var presets = AssetDatabase.FindAssets("t:" + typeof(RecorderControllerSettingsPreset).Name);

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
                        {
                            if (item.editor != null)
                                DestroyImmediate(item.editor);
                            
                            DeleteRecorder(item, false);
                        }
                        
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
            if (m_SelectedRecorderItem != null && m_SelectedRecorderItem.settings != null)
            {
                var presetReceiver = CreateInstance<PresetReceiver>();
                presetReceiver.Init(m_SelectedRecorderItem.settings, this);
                
                PresetSelector.ShowSelector(m_SelectedRecorderItem.settings, null, true, presetReceiver);
            }
        }

        void OnDestroy()
        {
            if (m_ControllerSettings != null)
            {
                m_ControllerSettings.Save();
                DestroyImmediate(m_ControllerSettings); // TODO Also destroy the RecorderSettings inside the prefs; // TODO Use Dispose
            }

            if (m_RecorderSettingsPrefsEditor != null)
                DestroyImmediate(m_RecorderSettingsPrefsEditor);
            
            Undo.undoRedoPerformed -= SaveAndRepaint;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void AddLastAndSelect(RecorderSettings recorder, string desiredName, bool enabled)
        {
            recorder.name = GetUniqueRecorderName(desiredName);
            recorder.enabled = enabled;
            m_ControllerSettings.AddRecorderSettings(recorder);

            m_RecordingListItem.AddAndSelect(CreateRecorderItem(recorder));
        }

        void DuplicateRecorder(RecorderItem item)
        {
            var candidate = item.settings;
            var copy = Instantiate(candidate);
            copy.OnAfterDuplicate();
            AddLastAndSelect(copy, candidate.name, candidate.enabled);
        }

        void DeleteRecorder(RecorderItem item, bool prompt)
        {
            if (!prompt || EditorUtility.DisplayDialog("Delete Recoder?",
                    "Are you sure you want to delete '" + item.settings.name + "' ?", "Delete"))
            {

                var s = item.settings;
                m_ControllerSettings.RemoveRecorder(s);
                UnityHelpers.Destroy(s, true);
                UnityHelpers.Destroy(item.editor, true);
                m_RecordingListItem.Remove(item);
            }
        }

        void OnAddNewRecorder(RecorderInfo info)
        {           
            var recorder = RecordersInventory.CreateDefaultRecorderSettings(info.settingsType);  
            AddLastAndSelect(recorder, ObjectNames.NicifyVariableName(info.displayName), true);
            
            m_RecorderSettingPanel.Dirty(ChangeType.All);
        }

        string GetUniqueRecorderName(string desiredName)
        {
            return ObjectNames.GetUniqueName(m_ControllerSettings.recorderSettings.Select(r => r.name).ToArray(),
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
            m_SelectedRecorderItem = m_RecordingListItem.selection;
            
            foreach (var r in m_RecordingListItem.items)
            {
                r.SetItemSelected(m_SelectedRecorderItem == r);
            }

            if (m_SelectedRecorderItem != null)
                m_RecorderSettingPanel.Dirty(ChangeType.Layout | ChangeType.Styles);
            
            Repaint();
        }

        bool HaveActiveRecordings()
        {
            return m_ControllerSettings.recorderSettings.Any(r => r.enabled);
        }

        void UpdateRecordingProgressGUI()
        {
            if (m_State == State.Idle)
            {
                if (!HaveActiveRecordings())
                {
                    EditorGUILayout.LabelField(new GUIContent("No active recorder"));
                }
                else
                {
                    var msg = CheckRecordersIncompatibility();
                    if (string.IsNullOrEmpty(msg))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Ready to start recording"));
                    }
                    else
                    {
                        var c = GUI.color;
                        
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField(new GUIContent(msg));
                        
                        GUI.color = c;
                    }
                }
                
                return;
            }

            if (m_State == State.WaitingForPlayModeToStartRecording)
            {
                EditorGUILayout.LabelField(new GUIContent("Waiting for Playmode to start..."));
                return;
            }
            
            var recordingSessions = m_RecorderController.GetRecordingSessions();

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
                    var label = session.currentFrameStartTS < settings.startTime
                        ? string.Format("Skipping first {0} second(s)...", settings.startTime)
                        : string.Format("{0} Frame(s) processed", session.frameIndex - settings.startFrame + 1);
                    EditorGUI.ProgressBar(progressBarRect, (float) session.currentFrameStartTS / (settings.endTime.Equals(0.0f) ? 0.0001f : settings.endTime), label);
                    
                    break;
                }
            }
        }
    }
}
