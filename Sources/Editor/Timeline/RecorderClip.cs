using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Recorder.Timeline
{
    [System.ComponentModel.DisplayName("Recorder Clip")]
    public class RecorderClip : PlayableAsset, ITimelineClipAsset
    {
        public delegate void RecordingClipDoneDelegate(RecorderClip clip);

        public static RecordingClipDoneDelegate OnClipDone;

//        [SerializeField] public RecorderSettings m_Settings;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RecorderPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
//            if (m_Settings != null && UnityHelpers.IsPlaying())
//            {
//                
//                //var RecordersInventory.CreateDefaultRecorderSettings(m_Settings.GetType());
//                behaviour.session = new RecordingSession()
//                {
//                    //m_Recorder = RecordersInventory.CreateDefaultRecorderSettings(m_Settings.GetType()),
//                    //m_RecorderGO = SceneHook.GetRecorderHost(true),
//                };
//                behaviour.OnEnd = () =>
//                {
//                    try
//                    {
//                        if (OnClipDone != null) OnClipDone(this);     
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.Log("OnClipDone call back generated an exception: " + ex.Message );
//                        Debug.LogException(ex);
//                    }
//                };
//            }
            return playable;
        }

        public virtual void OnDestroy()
        {
            //UnityHelpers.Destroy( m_Settings, true );
        }
    }
}
