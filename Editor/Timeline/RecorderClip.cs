using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Recorder.Timeline
{
    [DisplayName("Recorder Clip")]
    public class RecorderClip : PlayableAsset, ITimelineClipAsset
    {
        public delegate void RecordingClipDoneDelegate(RecorderClip clip);

        public static RecordingClipDoneDelegate OnClipDone;

        [SerializeField]
        public RecorderSettings m_Settings;
        
        SceneHook m_SceneHook = new SceneHook(Guid.NewGuid().ToString());

        public Type recorderType
        {
            get { return m_Settings == null ? null : RecordersInventory.GetRecorderInfo(m_Settings.GetType()).recorderType; }
        }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RecorderPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            if (recorderType != null && UnityHelpers.IsPlaying())
            {
                behaviour.session = m_SceneHook.CreateRecorderSession(m_Settings, false);
                
                behaviour.OnEnd = () =>
                {
                    try
                    {
                        if (OnClipDone != null) OnClipDone(this);     
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("OnClipDone call back generated an exception: " + ex.Message );
                        Debug.LogException(ex);
                    }
                };
            }
            return playable;
        }

        public virtual void OnDestroy()
        {
            UnityHelpers.Destroy( m_Settings, true );
        }
    }
}