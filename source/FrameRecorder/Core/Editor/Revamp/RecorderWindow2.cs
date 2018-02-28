using System;
using System.Collections.Generic;
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
using Random = UnityEngine.Random;

namespace UnityEditor.Recorder
{
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

        public void OnEnable()
        {
            Random.InitState(1337 + 42);

            var root = this.GetRootVisualContainer();

            int kMargin = 50;
            int kPadding = 10;
            int kBoxSize = 100;

            var mainControls = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flexDirection = FlexDirection.Row,
                }
            };
            root.Add(mainControls);
            
            var controlLeftPane = new VisualContainer
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

            var controlRightPane = new VisualContainer
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
                    width = 96.0f,
                    height = 85.0f,
                }
            };

            controlLeftPane.Add(recordIcon);


            var leftButtonsStack = new VisualContainer
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

            var recordModeComboBox = FieldWithLabel("Record Mode:", new EnumField(DurationMode.Manual));           

            leftButtonsStack.Add(recordModeComboBox);

            controlLeftPane.Add(leftButtonsStack);
            
            var rightButtonsStack = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    paddingTop = kPadding,
                    paddingBottom = kPadding,                    
                    flexDirection = FlexDirection.Column,
                }
            };
            
            rightButtonsStack.Add(new Label("Frame Rate"));
            rightButtonsStack.Add(FieldWithLabel("Playback", new EnumField(FrameRateMode.Constant), 30.0f));
            rightButtonsStack.Add(FieldWithLabel("Target fps", new EnumField(EFrameRate.FR_30), 30.0f));
            rightButtonsStack.Add(FieldWithLabel("Sync. framerate", new UnityEngine.Experimental.UIElements.Toggle(() => Debug.Log("Toggled Yo!")) { on = true }, 30.0f));
            
            
            controlRightPane.Add(rightButtonsStack);
            

            var parameters = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flex = 1.0f,
                }
            };

            var recordingAndParameters = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flex = 1.0f,
                    alignSelf = Align.Stretch,
                    flexDirection = FlexDirection.Row,

                }
            };
            
            var recordingsPanel = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    width = 200.0f,
                }
            };

            recordingAndParameters.Add(recordingsPanel);
            recordingAndParameters.Add(parameters);

            root.Add(recordingAndParameters);

            var recordingControl = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(0.8f),
                    height = 20,
                }
            };
            
            recordingControl.Add(new Label("+ Add New Recordings"));
            
            var recordings = new ScrollView
            {
                
                style =
                {
//                    backgroundColor = RandomColor(),
//                    width = 200.0f,
                    flex = 1.0f,
                    flexDirection = FlexDirection.Column,
                }
            };
            
            for(var i = 0; i < 20; ++i)
                recordings.Add(Record("Recording [" + i + "]"));

            recordingsPanel.Add(recordingControl);
            recordingsPanel.Add(recordings);

            var parametersControl = new VisualContainer
            {
                style =
                {
                    backgroundColor = RandomColor(0.8f),
                    height = 30,
                }
            };

            parameters.Add(parametersControl);
        }

        static VisualElement Record(string name)
        {
            var container = new VisualContainer
            {
                style = { flex = 1.0f, flexDirection = FlexDirection.Row, backgroundColor = RandomColor() } 
            };
            
            container.Add( new UnityEngine.Experimental.UIElements.Toggle(() => { }));
            container.Add(new Label(name));

            return container;
        }

        static VisualElement FieldWithLabel(string label, VisualElement field, float indent = 0.0f)
        {
            var container = new VisualContainer
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
