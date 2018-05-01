using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{   
    public abstract class RecorderEditor : Editor
    {
        SerializedProperty m_CaptureEveryNthFrame;
        SerializedProperty m_FileNameGenerator;

        static Texture2D s_SeparatorTexture;
        static readonly Color s_SeparatorColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);

        protected virtual void OnEnable()
        {
            if (target != null)
            {               
                var pf = new PropertyFinder<RecorderSettings>(serializedObject);
                m_CaptureEveryNthFrame = pf.Find(x => x.captureEveryNthFrame);
                m_FileNameGenerator = pf.Find(w => w.fileNameGenerator);
                
                s_SeparatorTexture = Resources.Load<Texture2D>("vertical_gradient");
            }
        }
        
        static void DrawSeparator()
        {
            EditorGUILayout.Separator();
            
            var r = EditorGUILayout.GetControlRect();
            r.xMin -= 10.0f;
            r.xMax += 10.0f;
            r.yMin += 5.0f;
            r.height = 10;
            
            var orgColor = GUI.color;
            GUI.color = s_SeparatorColor;
            GUI.DrawTexture(r, s_SeparatorTexture);
            GUI.color = orgColor;
            
            EditorGUILayout.Separator();
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            
            FileTypeAndFormatGUI();
            
            DrawSeparator();
            
            NameAndPathGUI();

            ImageRenderOptionsGUI();
            
            EditorGUILayout.Separator();
            
            ExtraOptionsGUI();
            
            EditorGUILayout.Separator();
            
            OnEncodingGui();

            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();
            
            if (GUI.changed)
                ((RecorderSettings) target).SelfAdjustSettings();

            OnValidateSettingsGUI();
        }

        protected virtual void OnValidateSettingsGUI()
        {
            var errors = new List<string>();
            if (!((RecorderSettings) target).ValidityCheck(errors))
            {
                foreach (var error in errors)
                    EditorGUILayout.HelpBox(error, MessageType.Warning);
            }
        }

        protected virtual void NameAndPathGUI()
        {
            EditorGUILayout.PropertyField(m_FileNameGenerator, new GUIContent("File Name"));
        }

        protected virtual void ImageRenderOptionsGUI()
        {
            var recorder = (RecorderSettings) target;
           
            foreach (var inputsSetting in recorder.inputsSettings)
            {
                var p = GetInputSerializedProperty(serializedObject, inputsSetting);
                
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(p, new GUIContent("Capture"));
            }
        }
        
        static SerializedProperty GetInputSerializedProperty(SerializedObject owner, object fieldValue)
        {
            var targetObject = (object)owner.targetObject;
            var type = targetObject.GetType();

            foreach (var info in InputSettingsSelector.GetInputFields(type))
            {
                if (info.GetValue(targetObject) == fieldValue)
                {
                    return owner.FindProperty(info.Name);
                }

                if (typeof(InputSettingsSelector).IsAssignableFrom(info.FieldType))
                {
                    var selector = info.GetValue(targetObject);
                    var fields = InputSettingsSelector.GetInputFields(selector.GetType());
                    var selectorInput = fields.FirstOrDefault(i => i.GetValue(selector) == fieldValue);
                    
                    if (selectorInput != null)
                    {
                        return owner.FindProperty(info.Name);
                    }
                }
            }
            
            return null;
        }

        protected virtual void ExtraOptionsGUI()
        {
            if (((RecorderSettings)target).frameRatePlayback == FrameRatePlayback.Variable) // TODO Fix condition. Video does not support variable frameRate
                EditorGUILayout.PropertyField(m_CaptureEveryNthFrame, new GUIContent("Render Step Frame"));
        }

        protected virtual void FileTypeAndFormatGUI()
        {   
        }

        protected virtual void OnEncodingGui()
        {
        }
    }
}

