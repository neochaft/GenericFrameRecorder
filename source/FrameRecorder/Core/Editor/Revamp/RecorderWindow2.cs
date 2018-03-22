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
    public class RecorderWindow2 : EditorWindow, ISerializationCallbackReceiver
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
        [SerializeField] float m_RecordingsPanelWidth = 200.0f;
        RecorderEditor m_RecorderEditor;
        VisualElement m_RecorderHeader;
        Button m_StartRecordButton;
        PanelSplitter m_PanelSplitter;

        [SerializeField]
        int m_SelectedRecorderItemIndex = 0;

        [SerializeField]
        GlobalSettings m_GlobalSettings;
        
        [SerializeField]
        RecordersList m_RecordersList;
        
        enum State
        {
            Idle,
            WaitingForPlayModeToStartRecording,
            Recording
        }
        
        State m_State = State.Idle;
        int m_FrameCount = 0;

        GlobalSettingsEditor m_GlobalSettingsEditor;
        
        //RecorderWindowSettings m_WindowSettingsAsset;
        
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

            int kMargin = 50;
            int kPadding = 10;
            int kBoxSize = 100;

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
            
            //mainControls.style.height = 300.0f; // TODO Remove!

            var recordIcon = new Image
            {
                image = Resources.Load<Texture2D>("recorder_icon"),

                style =
                {
                    backgroundColor = RandomColor(),
                    width = 96.0f,
                    height = 85.0f,
                }
            };

            controlLeftPane.Add(recordIcon);


            var leftButtonsStack = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    paddingTop = kPadding,
                    paddingBottom = kPadding,
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };

            m_StartRecordButton = new Button(OnRecordButtonClick)
            {
                text = "Start Recording"
            };

            leftButtonsStack.Add(m_StartRecordButton);

            m_GlobalSettings = GlobalSettings.instance; //LoadSettings<GlobalSettings>("GlobalSettings"); 
            
            m_GlobalSettingsEditor = (GlobalSettingsEditor) Editor.CreateEditor(m_GlobalSettings);
                
            var globalOptions = new IMGUIContainer(() =>
            {
                if (m_GlobalSettingsEditor.RecordModeGUI())
                    EditorUtility.SetDirty(m_GlobalSettingsEditor);
            })
            {
                style = {flex = 1.0f,}
            };

            leftButtonsStack.Add(globalOptions);

            controlLeftPane.Add(leftButtonsStack);
            
            var rightButtonsStack = new IMGUIContainer(() =>
            {
                if (m_GlobalSettingsEditor.FrameRateGUI())
                    EditorUtility.SetDirty(m_GlobalSettingsEditor);
            })
            {
                style = {flex = 1.0f,}
            };
            
            controlRightPane.Add(rightButtonsStack);
            

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

            var addNewRecord = new Label("+ Add New Recordings");
            
            //var newRecordMenu = new PopupField<string>(GetRecorders().Select(t => "New " + ObjectNames.NicifyVariableName(t.Name)).ToList(), 0);
            
            //newRecordMenu.AddItem();PopupField<string>(GetRecorders().Select(t => "New " + ObjectNames.NicifyVariableName(t.Name)).ToList(), 0);
            
            addNewRecord.RegisterCallback<MouseUpEvent>(evt =>
            {               
                var newRecordMenu = new GenericMenu();

                var recorderList = RecordersInventory.ListRecorders();
////                //var recorderList = GetRecorders();
////                foreach (var info in recorderList)
////                {
////                    newRecordMenu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
////                }
//
//                var recorderTypes = GetRecorders();
                
                foreach (var info in recorderList)
                {
                    newRecordMenu.AddItem(new GUIContent(info.displayName), false, data => OnAddNewRecorder((RecorderInfo) data), info);
                }
                
                newRecordMenu.ShowAsContext();
            });
            //newRecordMenu.AppendAction("Action 1", evt => Debug.Log("Yolo 1"), ContextualMenu.MenuAction.AlwaysEnabled);
            
            recordingControl.Add(addNewRecord);
            //recordingControl.Add(newRecordMenu);
            
            //recordingControl.RegisterCallback<MouseUpEvent>(OnAddNewRecorder);
            
            //var m = new ContextualMenuManipulator(MyDelegate);
            //m.target = re;
            
            m_Recordings = new ScrollView
            {
                
                style =
                {
//                    backgroundColor = RandomColor(),
//                    width = 200.0f,
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };

            m_Recordings.contentContainer.style.positionLeft = 0;
            m_Recordings.contentContainer.style.positionRight = 0;
            
            //m_recordings.RegisterCallback<PostLayoutEvent>(AdjustScrollViewWidth);
            
            //for(var i = 0; i < 20; ++i)
            //    recordings.Add(Record("Recording [" + i + "]"));

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
            m_RecorderHeader = new IMGUIContainer(OnRecorderHeader)
            {
                style = { flex = 1.0f }
            };
            
            parametersControl.Add(m_RecorderHeader);
            

            m_SettingsPanel.Add(parametersControl);
            
//            m_RecorderInspector = new IMGUIContainer(OnGUIHandler)
//            {
//                style =
//                {
//                    flex = 1.0f
//                }
//            };
//            
//            m_Parameters.Add(m_RecorderInspector);
            
            
            // Load recorders
            if (m_RecordersList == null)
                m_RecordersList = LoadSettings<RecordersList>("RecordersList");

            foreach (var recorderSettings in m_RecordersList.recorders)
            {
                var info = RecordersInventory.GetRecorderInfo(recorderSettings.recorderType);
                m_Recordings.Add(new RecorderItem(recorderSettings, info.iconName, OnRecordMouseUp));
            }
            
            SelectRecording(m_Recordings.Children().ElementAt(m_SelectedRecorderItemIndex));
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
//            else
//            {
//                m_State = State.Idle;
//            }

        }
        
        void DelayedStartRecording()
        {
            StartRecording(true);
        }

        void StartRecording(bool autoExitPlayMode)
        {         
            var go = SceneHook.HookupRecorder();

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
                
                var session = new RecordingSession
                {
                    m_Recorder = RecordersInventory.GenerateNewRecorder(settings.recorderType, settings),
                    m_RecorderGO = go,
                };
                
                var component = go.AddComponent<RecorderComponent>();
                component.session = session;
                component.autoExitPlayMode = autoExitPlayMode;
                
                sessions.Add(session);
            }
            
            var success = sessions.All(s => s.SessionCreated() && s.BeginRecording());

            if (success)
                m_State = State.Recording;
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
//                    var errors = new List<string>();
//                    using (new EditorGUI.DisabledScope(!m_Editor.ValidityCheck(errors)))
//                    {
//                        if (GUILayout.Button("Start Recording"))
//                            StartRecording();
//                    }

                    m_StartRecordButton.text = "Stop Recording";
                    
                    
                    m_State = State.WaitingForPlayModeToStartRecording;
                    GameViewSize.DisableMaxOnPlay();
                    EditorApplication.isPlaying = true;
                    m_FrameCount = Time.frameCount;
                    
                    break;
                }

                case State.WaitingForPlayModeToStartRecording:
                //{
                    //m_startRecordButton.text = "Start Recording";

//                    using (new EditorGUI.DisabledScope(Time.frameCount - m_FrameCount < 5))
//                    {
//                        if (GUILayout.Button("Stop Recording"))
//                            StopRecording();
//                    }
                    //break;
                //}

                case State.Recording:
                {   
                    StopRecording();
                    
                    m_StartRecordButton.text = "Start Recording";
                    
//                    var recorderGO = SceneHook.FindRecorder((RecorderSettings)m_Editor.target);
//                    if (recorderGO == null)
//                    {
//                        GUILayout.Button("Start Recording"); // just to keep the ui system happy.
//                        m_State = State.Idle;
//                        m_FrameCount = 0;
//                    }
//                    else
//                    {
//                        if (GUILayout.Button("Stop Recording"))
//                            StopRecording();
//                        //UpdateRecordingProgress(recorderGO); // TODO yo
//                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
              
        void StopRecording()
        {
            foreach (RecorderItem recorderItem in m_Recordings.Children())
            {
                //if (m_Editor != null)
                {
                    var settings = recorderItem.settings; //(RecorderSettings)m_Editor.target;
                    if (settings != null)
                    {
                        var recorderGO = SceneHook.FindRecorder(settings);
                        if (recorderGO != null)
                        {
                            UnityHelpers.Destroy(recorderGO);
                        }
                    }
                }    
            }
            
            m_FrameCount = 0;
            m_State = State.Idle;
        }

        void OnRecorderHeader()
        {
            if (m_RecorderEditor != null)
            {
                var rec = (RecorderSettings)m_RecorderEditor.target;
                
                //var r = EditorGUILayout.GetControlRect();
                //Presets.PresetSelector.DrawPresetButton(r, new Object[] {rec});
                
                
                EditorGUILayout.LabelField("Recording Type",  rec.GetType().Name);
                
                m_RecorderEditor.OnInspectorGUI();
                //m_RecorderEditor.CaptureOptionsGUI();
                
                //m_RecorderEditor.OnEncodingGroupGui();
                //m_RecorderEditor.OutputPathsGUI();
                
            }
            else
            {
                EditorGUILayout.LabelField("Nothing selected");
            }
        }

        //IMGUIContainer m_RecorderInspector;

//        void OnGUIHandler()
//        {
//            if (m_RecorderEditor != null)
//            {
//                m_RecorderEditor.OnInspectorGUI();
//            }
//        }

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
              SelectRecording(recorder);
//            //m_parameters.Dirty(ChangeType.Layout);
//            evt.StopImmediatePropagation();
            }
            else
            {             
                var contextMenu = new GenericMenu();
                
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
                
                contextMenu.ShowAsContext();
            }
        }

        void SelectRecording(int index)
        {
            m_RecorderEditor = null;

            var selected = m_Recordings.Children().ElementAt(index);

            SelectRecording(selected);
        }
        
        void SelectRecording(VisualElement selected)
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
                    r.SetSelected(true);
                    m_SelectedRecorderItemIndex = i;
                }
                else
                {
                    r.SetSelected(false);
                    ++i;
                }
            }

            if (m_RecorderEditor != null)
            {
                //m_RecorderInspector.Dirty(ChangeType.Layout);
                m_RecorderHeader.Dirty(ChangeType.Layout);
            }

        }
        
        class RecorderItem : VisualElement
        {
            public RecorderSettings settings { get; private set; }
            public RecorderEditor editor { get; private set; }

            //public UnityEngine.Recorder.Recorder recorder { get; private set; }

            Color m_Color;
            //public string title { get; set; }

            public RecorderItem(RecordersList recordersList, Type recorderType, string recorderName, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
            {
                var savedSettings = RecordersInventory.GenerateRecorderInitialSettings(recordersList, recorderType);
                savedSettings.name = recorderName;
                
                recordersList.Add(savedSettings);

                Init(savedSettings, iconName, onRecordMouseUp);
            }
            
            public RecorderItem(RecorderSettings savedSettings, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
            {
                Init(savedSettings, iconName, onRecordMouseUp);
            }
            
            void Init(RecorderSettings savedSettings, string iconName, EventCallback<MouseUpEvent> onRecordMouseUp)
            {
                settings = savedSettings;

                //var recorderType = settings.recorderType;

                //settings.assetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settings));
                //settings.inputsSettings.AddRange(settings.GetDefaultInputSettings());
                
                //recorder = 
                editor = (RecorderEditor)Editor.CreateEditor(settings);

                style.flex = 1.0f;
                style.flexDirection = FlexDirection.Row;
                style.backgroundColor = m_Color = RandomColor();

                //container.RegisterCallback<MouseUpEvent>(OnRecordMouseUp);
                var toggle = new Toggle(null) { on = settings.enabled };
                
                toggle.OnToggle(() =>
                {
                    settings.enabled = toggle.on;
                });
                
                
                Add(toggle);


                var t = Resources.Load<Texture2D>(iconName); // TODO Cache?

                if (t != null) // TODO Use a default image?
                {
                    var recordIcon = new Image
                    {
                        image = t,

                        style =
                        {
                            backgroundColor = RandomColor(),
                            height = 16.0f,
                            width = 16.0f,
                        }
                    };
                    
                    Add(recordIcon);
                }

                
                //Add(new Label(title));
                var titleField = new TextField { text = settings.name };
                titleField.OnValueChanged(evt => settings.name = evt.newValue);
                Add(titleField);
                
                
                RegisterCallback(onRecordMouseUp);
                //RegisterCallback<MouseUpEvent>(onRecordContextMenu);
            }

            public void SetSelected(bool value)
            {
                style.backgroundColor = value ? Color.white : m_Color;

            }
        }
        
        static IEnumerable<Type> GetRecorders()
        {
            //var type = typeof(RecorderBase);
            var type = typeof(RecorderSettings);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract && type.IsAssignableFrom(p));
            return types;
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
