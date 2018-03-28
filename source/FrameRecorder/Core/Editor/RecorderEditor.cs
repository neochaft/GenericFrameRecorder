using System;
using System.Collections.Generic;
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

        protected class InputEditorState
        {
            readonly InputEditor.IsFieldAvailableDelegate m_Validator;
            public InputEditor editor { get; private set; }

            RecorderInputSetting m_SettingsObj;

            public RecorderInputSetting settingsObj
            {
                get { return m_SettingsObj; }
                set
                {
                    m_SettingsObj = value;
                    if (editor != null)
                        UnityHelpers.Destroy(editor);

                    editor = CreateEditor(m_SettingsObj) as InputEditor;
                    if (editor != null)
                        editor.isFieldAvailableForHost = m_Validator;
                }
            }

            public InputEditorState(InputEditor.IsFieldAvailableDelegate validator, RecorderInputSetting settings)
            {
                m_Validator = validator;
                settingsObj = settings;
            }
        }

        protected List<InputEditorState> m_InputEditors;
        readonly List<string> m_SettingsErrors = new List<string>();
        InputSelector m_InputSelector;

        SerializedProperty m_CaptureEveryNthFrame;
        SerializedProperty m_DestinationPath;
        SerializedProperty m_BaseFileName;

        protected virtual void OnEnable()
        {
            if (target != null)
            {
                m_InputEditors = new List<InputEditorState>();
                
                var pf = new PropertyFinder<UnityEngine.Recorder.RecorderSettings>(serializedObject);
                m_CaptureEveryNthFrame = pf.Find(x => x.captureEveryNthFrame);
                m_DestinationPath = pf.Find(w => w.destinationPath);
                m_BaseFileName = pf.Find(w => w.baseFileName);

                m_InputSelector = new InputSelector((UnityEngine.Recorder.RecorderSettings) target);

                BuildInputEditors();
            }
        }

        void BuildInputEditors()
        {
            var rs = (UnityEngine.Recorder.RecorderSettings) target;
            if (!rs.inputsSettings.hasBrokenBindings && rs.inputsSettings.Count == m_InputEditors.Count)
                return;

            if (rs.inputsSettings.hasBrokenBindings)
                rs.BindSceneInputSettings();

            foreach (var editor in m_InputEditors)
                UnityHelpers.Destroy(editor.editor);
            m_InputEditors.Clear();

            foreach (var input in rs.inputsSettings)
                m_InputEditors.Add(new InputEditorState(GetFieldDisplayState, input));
        }

        public bool ValidityCheck(List<string> errors)
        {
            return ((UnityEngine.Recorder.RecorderSettings) target).ValidityCheck(errors)
                && ((UnityEngine.Recorder.RecorderSettings) target).isPlatformSupported;
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
            
            BuildInputEditors();

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            
            FileTypeAndFormatGUI();
            CaptureOptionsGUI();
            
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            
            NameAndPathGUI();

            ImageRenderOptionsGUI();
            
            ExtraOptionsGUI();
            
            EditorGUILayout.Separator();
            
            OnEncodingGroupGui();

            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();

            ((UnityEngine.Recorder.RecorderSettings) target).SelfAdjustSettings();

            OnValidateSettingsGUI();
        }

        protected virtual void OnValidateSettingsGUI()
        {
            m_SettingsErrors.Clear();
            if (!((UnityEngine.Recorder.RecorderSettings) target).ValidityCheck(m_SettingsErrors))
            {
                foreach (var error in m_SettingsErrors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Warning);
                }
            }
        }

        void ChangeInputSettings(int atIndex, RecorderInputSetting newSettings)
        {
            if (newSettings != null)
            {
                var inputs = ((UnityEngine.Recorder.RecorderSettings) target).inputsSettings;
                inputs.ReplaceAt(atIndex, newSettings);
                m_InputEditors[atIndex].settingsObj = newSettings;
            }
            else if (m_InputEditors.Count == 0)
            {
                throw new Exception("Source removal not implemented");
            }
        }

        protected virtual void CaptureOptionsGUI()
        {
            var inputs = ((UnityEngine.Recorder.RecorderSettings) target).inputsSettings;
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                if (m_InputSelector.OnInputGui(i, ref input))
                    ChangeInputSettings(i, input);

                m_InputEditors[i].editor.CaptureOptionsGUI();
            }
        }

        protected virtual void OnInputGui(int inputIndex)
        {
            m_InputEditors[inputIndex].editor.OnInspectorGUI();
            m_InputEditors[inputIndex].editor.OnValidateSettingsGUI();
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
            var inputs = ((UnityEngine.Recorder.RecorderSettings) target).inputsSettings;

            for (int i = 0; i < inputs.Count; i++)
            {
                EditorGUILayout.Separator();
                OnInputGui(i);
            }
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

