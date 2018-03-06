using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Recorder.Input;
using UnityEngine.Timeline;
using UnityEngine.UI;
using Button = UnityEngine.Experimental.UIElements.Button;
using Image = UnityEngine.Experimental.UIElements.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace UnityEditor.Recorder
{   
    [CustomPropertyDrawer(typeof(FrameRate))]
    class FrameRateProperyDrawer : PropertyDrawer
    {
        readonly GUIContent[] m_DisplayNames;
        
        public FrameRateProperyDrawer()
        {
            var displayNames = new List<GUIContent>();

            foreach (FrameRate frameRate in Enum.GetValues(typeof(FrameRate)))
            {
                displayNames.Add(new GUIContent(ToLabel(frameRate)));
            }

            m_DisplayNames = displayNames.ToArray();
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            property.intValue = EditorGUI.Popup(position, label, property.intValue, m_DisplayNames);
                
            EditorGUI.EndProperty();
        }
        
        static string ToLabel(FrameRate frameRate)
        {
            switch (frameRate)
            {
                case FrameRate.FR_23:
                    return "23.97";
                case FrameRate.FR_24:
                    return "Film (24)";
                case FrameRate.FR_25:
                    return "PAL (25)";
                case FrameRate.FR_29:
                    return "NTSC (29.97)";
                case FrameRate.FR_30:
                    return "30";
                case FrameRate.FR_50:
                    return "50";
                case FrameRate.FR_59:
                    return "59.94" ;
                case FrameRate.FR_60:
                    return "60";
                case FrameRate.FR_CUSTOM:
                    return "Custom";
                    
                default:
                    return "unknown";
            }
        }       
    }

    [CustomEditor(typeof(GlobalSettings))]
    [CanEditMultipleObjects]
    class GlobalSettingsEditor : Editor
    {
        SerializedProperty m_RecordModeProperty;
        
        SerializedProperty m_PlaybackProperty;
        SerializedProperty m_FrameRateTypeProperty;
        SerializedProperty m_CustomFrameRateValueProperty;
        
        SerializedProperty m_StartFrameProperty;
        SerializedProperty m_EndFrameProperty;
        SerializedProperty m_StartTimeProperty;
        SerializedProperty m_EndTimeProperty;
        
        SerializedProperty m_SynchFrameRateProperty;

        GenericMenu m_FrameRateMenu;

        static class Styles
        {
            public static readonly GUIContent sRecordModeLabel  = new GUIContent("Record Mode");
            public static readonly GUIContent sSingleFrameLabel = new GUIContent("Frame #");
            public static readonly GUIContent sFirstFrameLabel  = new GUIContent("First frame");
            public static readonly GUIContent sLastFrameLabel   = new GUIContent("Last frame");
            public static readonly GUIContent sStartTimeLabel   = new GUIContent("Start (sec)");
            public static readonly GUIContent sEndTimeLabel     = new GUIContent("End (sec)");
            
            public static readonly GUIContent sFrameRateTitle   = new GUIContent("Frame Rate");
            public static readonly GUIContent sPlaybackLabel    = new GUIContent("Playback");
            public static readonly GUIContent sTargetFPSLabel   = new GUIContent("Target Frame Rate");
            public static readonly GUIContent sMaxFPSLabel      = new GUIContent("Max Frame Rate");
            public static readonly GUIContent sSyncFPSLabel     = new GUIContent("Sync. Frame Rate");
            public static readonly GUIContent sValueLabel       = new GUIContent("Value");
        }

        void OnEnable()
        {          
            m_RecordModeProperty = serializedObject.FindProperty("m_RecordMode");
            m_PlaybackProperty = serializedObject.FindProperty("m_FrameRatePlayback");
            m_FrameRateTypeProperty  = serializedObject.FindProperty("m_FrameRateType");
            m_CustomFrameRateValueProperty = serializedObject.FindProperty("m_CustomFrameRateValue");
            m_StartFrameProperty = serializedObject.FindProperty("m_StartFrame");
            m_EndFrameProperty = serializedObject.FindProperty("m_EndFrame");
            m_StartTimeProperty = serializedObject.FindProperty("m_StartTime");
            m_EndTimeProperty = serializedObject.FindProperty("m_EndTime");
            m_SynchFrameRateProperty = serializedObject.FindProperty("m_SynchFrameRate");
        }

        public bool RecordModeGUI()
        {           
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(m_RecordModeProperty, Styles.sRecordModeLabel);

            ++EditorGUI.indentLevel;
            
            switch ((RecordMode)m_RecordModeProperty.enumValueIndex)
            {
                case RecordMode.Manual:
                {
                    // Nothing
                    break;
                }
                    
                case RecordMode.SingleFrame:
                {
                    EditorGUILayout.PropertyField(m_StartFrameProperty, Styles.sSingleFrameLabel);
                    m_EndFrameProperty.intValue = m_StartFrameProperty.intValue;
                    break;
                }
                    
                case RecordMode.FrameInterval:
                {
                    EditorGUILayout.PropertyField(m_StartFrameProperty, Styles.sFirstFrameLabel);
                    EditorGUILayout.PropertyField(m_EndFrameProperty, Styles.sLastFrameLabel);
                    break;
                }
                    
                case RecordMode.TimeInterval:
                {
                    EditorGUILayout.PropertyField(m_StartTimeProperty, Styles.sStartTimeLabel);
                    EditorGUILayout.PropertyField(m_EndTimeProperty, Styles.sEndTimeLabel);
                    break;
                }
                    
            }
            
            --EditorGUI.indentLevel;            
            
            serializedObject.ApplyModifiedProperties();
            
            return GUI.changed;
        }
        
        public bool FrameRateGUI()
        {           
            serializedObject.Update();
            
            EditorGUILayout.LabelField(Styles.sFrameRateTitle);
            
            ++EditorGUI.indentLevel;
            
            EditorGUILayout.PropertyField(m_PlaybackProperty, Styles.sPlaybackLabel);

            var variableFPS = m_PlaybackProperty.enumValueIndex == (int) FrameRatePlayback.Variable;
            
            EditorGUILayout.PropertyField(m_FrameRateTypeProperty, variableFPS ? Styles.sMaxFPSLabel : Styles.sTargetFPSLabel);

            //EditorGUILayout.EnumPopup((FrameRate) (m_FrameRateTypeProperty.enumValueIndex));

            if (m_FrameRateTypeProperty.enumValueIndex == (int) FrameRate.FR_CUSTOM)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CustomFrameRateValueProperty, Styles.sValueLabel);
                --EditorGUI.indentLevel;
            }
            
            if (variableFPS)
            {
                EditorGUILayout.PropertyField(m_SynchFrameRateProperty, Styles.sSyncFPSLabel);       
            }
            
            --EditorGUI.indentLevel;
            
            serializedObject.ApplyModifiedProperties();

            return GUI.changed;
        }
    }
    
    
    public class RecorderWindow2 : EditorWindow
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

        VisualElement m_recordings;
        VisualElement m_parameters;
        Editor m_recorderEditor;

        [SerializeField]
        GlobalSettings m_GlobalSettings;
        
        GlobalSettingsEditor m_GlobalSettingsEditor;
        
        RecorderWindowSettings m_WindowSettingsAsset;

        GlobalSettings LoadGlobalSettings()
        {
            if (m_GlobalSettings == null)
            {
                var candidates = AssetDatabase.FindAssets("t:GlobalSettings");
                if (candidates.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(candidates[0]);
                    m_GlobalSettings = AssetDatabase.LoadAssetAtPath<GlobalSettings>(path);
                    if (m_GlobalSettings == null)
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }
                if(m_GlobalSettings == null)
                {
                    m_GlobalSettings = CreateInstance<GlobalSettings>();
                    AssetDatabase.CreateAsset(m_GlobalSettings, "Assets/GlobalSettings.asset");
                    AssetDatabase.Refresh();
                }
            }

            return m_GlobalSettings;
        }
        
        public void OnEnable()
        {
//            if (m_WindowSettingsAsset == null)
//            {
//                var candidates = AssetDatabase.FindAssets("t:RecorderWindow2Settings");
//                if (candidates.Length > 0)
//                {
//                    var path = AssetDatabase.GUIDToAssetPath(candidates[0]);
//                    m_WindowSettingsAsset = AssetDatabase.LoadAssetAtPath<RecorderWindowSettings>(path);
//                    if (m_WindowSettingsAsset == null)
//                    {
//                        AssetDatabase.DeleteAsset(path);
//                    }
//                }
//                if(m_WindowSettingsAsset == null)
//                {
//                    m_WindowSettingsAsset = ScriptableObject.CreateInstance<RecorderWindowSettings>();
//                    AssetDatabase.CreateAsset(m_WindowSettingsAsset, FRPackagerPaths.GetRecorderRootPath() +  "/RecorderWindow2Settings.asset");
//                    AssetDatabase.Refresh();
//                }
//            }
            
            // TODO Restore current list
            
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

            var startRecordButton = new Button(() => { Debug.Log("Clicked Yo!"); })
            {
                text = "Start Recording Yo"
            };

            leftButtonsStack.Add(startRecordButton);

            //var recordModeComboBox = FieldWithLabel("Record Mode:", new EnumField(DurationMode.Manual));

            m_GlobalSettings = LoadGlobalSettings(); //CreateInstance<GlobalSettings>();
            //AssetDatabase.CreateAsset(m_GlobalSettings, "Assets/GlobalSettings.asset");
            //AssetDatabase.SaveAssets();
            
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
            

            m_parameters = new ScrollView
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flex = 1.0f,
                }
            };
            
            m_parameters.contentContainer.style.positionLeft = 0;
            m_parameters.contentContainer.style.positionRight = 0;

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
            
            var recordingsPanel = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    width = 200.0f,
                }
            };

            recordingAndParameters.Add(recordingsPanel);
            recordingAndParameters.Add(m_parameters);

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
                //var recorderList = GetRecorders();
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
            
            m_recordings = new ScrollView
            {
                
                style =
                {
//                    backgroundColor = RandomColor(),
//                    width = 200.0f,
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };

            m_recordings.contentContainer.style.positionLeft = 0;
            m_recordings.contentContainer.style.positionRight = 0;
            
            //m_recordings.RegisterCallback<PostLayoutEvent>(AdjustScrollViewWidth);
            
            //for(var i = 0; i < 20; ++i)
            //    recordings.Add(Record("Recording [" + i + "]"));

            recordingsPanel.Add(recordingControl);
            recordingsPanel.Add(m_recordings);

            var parametersControl = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(0.8f),
                    height = 30,
                    minWidth = 300.0f,
                }
            };

            m_parameters.Add(parametersControl);
            
            m_recorderInspector = new IMGUIContainer(OnGUIHandler)
            {
                style =
                {
                    flex = 1.0f
                }
            };
            
            m_parameters.Add(m_recorderInspector);
            
            
        }

        IMGUIContainer m_recorderInspector;

        void OnGUIHandler()
        {
            if (m_recorderEditor != null)
            {
                m_recorderEditor.OnInspectorGUI();
            }
        }

        void OnAddNewRecorder(RecorderInfo info)
        {
            m_recordings.Add(new RecorderItem(m_WindowSettingsAsset, info, OnRecordMouseUp));
        }

        void OnRecordMouseUp(MouseUpEvent evt)
        {
            if (evt.clickCount != 1)
                return;

            var recorder = (RecorderItem)evt.currentTarget;
            Debug.Log("Clicked on " + recorder.settings);
            m_recorderEditor = recorder.editor;
            m_recorderInspector.Dirty(ChangeType.Layout);
            //m_parameters.Dirty(ChangeType.Layout);
            evt.StopImmediatePropagation();
        }
        
        class RecorderItem : VisualElement
        {
            public RecorderSettings settings { get; private set; }
            public Editor editor { get; private set; }
            public UnityEngine.Recorder.Recorder recorder { get; private set; }
            
            public string title { get; set; }

            public RecorderItem(Object saveFileScriptableObject, RecorderInfo info, EventCallback<MouseUpEvent> onRecordMouseUp)
            {
                settings = RecordersInventory.GenerateRecorderInitialSettings(saveFileScriptableObject, info.recorderType);
                
                
                settings.assetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settings));
                settings.inputsSettings.AddRange( settings.GetDefaultInputSettings() );
                
                recorder = RecordersInventory.GenerateNewRecorder(info.recorderType, settings);
                editor = Editor.CreateEditor(settings);
                title = info.displayName; // TODO Add number or something?
                style.flex = 1.0f;
                style.flexDirection = FlexDirection.Row;
                style.backgroundColor = RandomColor();

                //container.RegisterCallback<MouseUpEvent>(OnRecordMouseUp);
                Add(new UnityEngine.Experimental.UIElements.Toggle(() => { }));
                //Add(new Label(title));
                var titleField = new TextField() {text = title};
                titleField.OnValueChanged(evt => title = evt.newValue);
                Add(titleField);
                
                
                RegisterCallback(onRecordMouseUp);
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
    }

    static class UIElementsHelper
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
