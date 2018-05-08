using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recorder
{
    class RecorderSelector
    {
        string[] m_RecorderNames;
        List<RecorderInfo> m_Recorders;
        bool m_SettingsAreAssets;

        public Type selectedRecorder { get; private set; }

        readonly Action m_SetRecorderCallback;

        public RecorderSelector(Action setRecorderCallback)
        {
            m_SetRecorderCallback = setRecorderCallback;
        }

        public void Init(RecorderSettings settings)
        {
//            if(settings != null)
//                SelectRecorder(settings.recorderType);
        }

        int GetRecorderIndex()
        {
            if (m_Recorders.Count == 0)
                return -1;
            
            for (int i = 0; i < m_Recorders.Count; i++)
                if (m_Recorders[i].recorderType == selectedRecorder)
                    return i;

            if (m_Recorders.Count > 0)
                return 0;
            
            return -1;
        }

        static Type GetRecorderFromIndex(int index)
        {
            return index >= 0 ? RecordersInventory.recorderInfos[index].recorderType : null;
        }

        public void OnGui()
        {
            // Recorder in group selection
            EditorGUILayout.BeginHorizontal();
            var oldIndex = GetRecorderIndex();
            var newIndex = EditorGUILayout.Popup("Selected recorder:", oldIndex, m_RecorderNames);
            SelectRecorder(GetRecorderFromIndex(newIndex));

            EditorGUILayout.EndHorizontal();
        }

        void SelectRecorder( Type newSelection )
        {
            if (selectedRecorder == newSelection)
                return;

            var recorderAttribs = newSelection.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            if (recorderAttribs.Length > 0 )
                Debug.LogWarning( "Recorder " + ((ObsoleteAttribute)recorderAttribs[0]).Message);

            selectedRecorder = newSelection;
            m_SetRecorderCallback();
        }
    }
}
