using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;
using UnityEngine.SocialPlatforms;

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
            InputEditor.IsFieldAvailableDelegate m_Validator;
            public bool visible;
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

                    editor = Editor.CreateEditor(m_SettingsObj) as InputEditor;
                    if (editor is InputEditor)
                        (editor as InputEditor).isFieldAvailableForHost = m_Validator;
                }
            }

            public InputEditorState(InputEditor.IsFieldAvailableDelegate validator, RecorderInputSetting settings)
            {
                m_Validator = validator;
                settingsObj = settings;
            }
        }

        protected List<InputEditorState> m_InputEditors;
        protected List<string> m_SettingsErrors = new List<string>();
        RTInputSelector m_RTInputSelector;

        SerializedProperty m_CaptureEveryNthFrame;
        SerializedProperty m_DestinationPath;
        SerializedProperty m_BaseFileName;

        protected virtual void OnEnable()
        {
            if (target != null)
            {
                m_InputEditors = new List<InputEditorState>();
                
                var pf = new PropertyFinder<RecorderSettings>(serializedObject);
                m_CaptureEveryNthFrame = pf.Find(x => x.m_CaptureEveryNthFrame);
                m_DestinationPath = pf.Find(w => w.m_DestinationPath);
                m_BaseFileName = pf.Find(w => w.m_BaseFileName);

                m_RTInputSelector = new RTInputSelector((RecorderSettings) target);

                BuildInputEditors();
            }
        }

        void BuildInputEditors()
        {
            var rs = (RecorderSettings) target;
            if (!rs.inputsSettings.hasBrokenBindings && rs.inputsSettings.Count == m_InputEditors.Count)
                return;

            if (rs.inputsSettings.hasBrokenBindings)
                rs.BindSceneInputSettings();

            foreach (var editor in m_InputEditors)
                UnityHelpers.Destroy(editor.editor);
            m_InputEditors.Clear();

            foreach (var input in rs.inputsSettings)
                m_InputEditors.Add(new InputEditorState(GetFieldDisplayState, input) { visible = true });
        }

        protected virtual void OnDisable() {}

        protected virtual void Awake() {}

        public bool ValidityCheck(List<string> errors)
        {
            return ((RecorderSettings) target).ValidityCheck(errors)
                && ((RecorderSettings) target).isPlatformSupported;
        }

        public bool showBounds { get; set; }

        bool m_FoldoutInput = true;
        bool m_FoldoutEncoder = true;
        bool m_FoldoutTime = true;
        bool m_FoldoutBounds = true;
        bool m_FoldoutOutput = true;

//        protected virtual void OnGroupGui()
//        {
//            //OnInputGroupGui();
//            //
//            OnEncodingGroupGui();
//            OutputPathsGUI();
//            //OnFrameRateGroupGui();
//            //OnBoundsGroupGui();
//            //OnExtraGroupsGui();
//        }

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

            ((RecorderSettings) target).SelfAdjustSettings();

            OnValidateSettingsGUI();
        }

        public virtual void OnValidateSettingsGUI()
        {
            m_SettingsErrors.Clear();
            if (!((RecorderSettings) target).ValidityCheck(m_SettingsErrors))
            {
                for (int i = 0; i < m_SettingsErrors.Count; i++)
                {
                    EditorGUILayout.HelpBox(m_SettingsErrors[i], MessageType.Warning);
                }
            }
        }

        protected void AddInputSettings(RecorderInputSetting inputSettings)
        {
            var inputs = ((RecorderSettings) target).inputsSettings;
            inputs.Add(inputSettings);
            m_InputEditors.Add(new InputEditorState(GetFieldDisplayState, inputSettings) { visible = true });
        }

        public void ChangeInputSettings(int atIndex, RecorderInputSetting newSettings)
        {
            if (newSettings != null)
            {
                var inputs = ((RecorderSettings) target).inputsSettings;
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
            var inputs = ((RecorderSettings) target).inputsSettings;
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                if (m_RTInputSelector.OnInputGui(i, ref input))
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
            var inputs = ((RecorderSettings) target).inputsSettings;

            //bool multiInputs = inputs.Count > 1;
            for (int i = 0; i < inputs.Count; i++)
            {
                EditorGUILayout.Separator();
                //if (multiInputs)
                //{
                //m_InputEditors[i].visible = EditorGUILayout.Foldout(m_InputEditors[i].visible, m_InputEditors[i].settingsObj.m_DisplayName ?? "Input " + (i + 1));
                //EditorGUI.indentLevel++;
                //}

                //if (m_InputEditors[i].visible)
                {
                    OnInputGui(i);
                }

                //if (multiInputs)
                //    EditorGUI.indentLevel--;
            }
        }

        protected virtual void ExtraOptionsGUI()
        {
            EditorGUILayout.PropertyField(m_CaptureEveryNthFrame, new GUIContent("Render Step Frame"));
        }

//        public virtual void CaptureOptionsGUI()
//        {
//            
//        }

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

