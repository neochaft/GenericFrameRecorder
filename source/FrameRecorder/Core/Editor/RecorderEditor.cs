using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cinemachine;
using UnityEngine;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public enum EFieldDisplayState
    {
        Enabled,
        Disabled,
        Hidden
    }

    public abstract class RecorderEditor : Editor
    {
        readonly List<string> m_SettingsErrors = new List<string>();

        SerializedProperty m_CaptureEveryNthFrame;
        SerializedProperty m_DestinationPath;
        SerializedProperty m_BaseFileName;

        protected virtual void OnEnable()
        {
            if (target != null)
            {               
                var pf = new PropertyFinder<RecorderSettings>(serializedObject);
                m_CaptureEveryNthFrame = pf.Find(x => x.captureEveryNthFrame);
                m_DestinationPath = pf.Find(w => w.destinationPath);
                m_BaseFileName = pf.Find(w => w.baseFileName);
            }
        }

        public bool ValidityCheck(List<string> errors)
        {
            return ((RecorderSettings) target).ValidityCheck(errors)
                && ((RecorderSettings) target).isPlatformSupported;
        }

        bool m_FoldoutInput = true;
        bool m_FoldoutEncoder = true;
        bool m_FoldoutTime = true;
        bool m_FoldoutBounds = true;
        bool m_FoldoutOutput = true;

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            
            FileTypeAndFormatGUI();
            CaptureOptionsGUI();
            
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            
            NameAndPathGUI();

            ImageRenderOptionsGUI();
            
            EditorGUILayout.Separator();
            
            ExtraOptionsGUI();
            
            EditorGUILayout.Separator();
            
            OnEncodingGroupGui();

            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();

            ((RecorderSettings) target).SelfAdjustSettings();

            OnValidateSettingsGUI();
        }

        protected virtual void OnValidateSettingsGUI()
        {
            m_SettingsErrors.Clear();
            if (!((RecorderSettings) target).ValidityCheck(m_SettingsErrors))
            {
                foreach (var error in m_SettingsErrors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Warning);
                }
            }
        }

        protected virtual void CaptureOptionsGUI()
        {
        }

        protected virtual void NameAndPathGUI()
        {
            EditorGUILayout.PropertyField(m_BaseFileName, new GUIContent("File Name"));
            EditorGUILayout.PropertyField(m_DestinationPath, new GUIContent("Path"));
            // TODO
            // Save to Folder
            // Folder Name
            // Export Settings
            //    File Name
            //    Folder Name
        }

        protected virtual void ImageRenderOptionsGUI()
        {
            var recorder = (RecorderSettings) target;
           
            foreach (var inputsSetting in recorder.inputsSettings)
            {
                var p = GetInputSerializedProperty(serializedObject, inputsSetting);
                
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(p, true);
            }
        }
        
        static SerializedProperty GetInputSerializedProperty(SerializedObject owner, object fieldValue)
        {
            var targetObject = (object)owner.targetObject;
            var type = targetObject.GetType();

            foreach (var info in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (info.GetValue(targetObject) == fieldValue)
                {
                    return owner.FindProperty(info.Name);
                }

                if (typeof(InputSettingsSelector).IsAssignableFrom(info.FieldType))
                {
                    var selector = info.GetValue(targetObject);
                    var fields = selector.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
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
            AddProperty(m_CaptureEveryNthFrame, () => EditorGUILayout.PropertyField(m_CaptureEveryNthFrame, new GUIContent("Render Step Frame")));
        }

        protected virtual void FileTypeAndFormatGUI()
        {   
        }

        protected virtual void OnEncodingGui()
        {
        }
        
        protected virtual void OnEncodingGroupGui()
        {
            m_FoldoutEncoder = EditorGUILayout.Foldout(m_FoldoutEncoder, "Encoding");
            if (m_FoldoutEncoder)
            {
                ++EditorGUI.indentLevel;
                OnEncodingGui();
                --EditorGUI.indentLevel;
            }
        }

        protected virtual EFieldDisplayState GetFieldDisplayState(SerializedProperty property)
        {
            return EFieldDisplayState.Enabled;
        }

        protected void AddProperty(SerializedProperty prop, Action action)
        {
            var state = GetFieldDisplayState(prop);
            if (state != EFieldDisplayState.Hidden)
            {
                using (new EditorGUI.DisabledScope(state == EFieldDisplayState.Disabled))
                    action();
            }
        }


    }
}

