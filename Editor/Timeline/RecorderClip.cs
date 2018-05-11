using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Recorder.Timeline
{
    [DisplayName("Recorder Clip")]
    class RecorderClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        public RecorderSettings m_Settings;

        readonly SceneHook m_SceneHook = new SceneHook(Guid.NewGuid().ToString());

        Type recorderType
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
            }
            return playable;
        }

        public void OnDestroy()
        {
            UnityHelpers.Destroy( m_Settings, true );
        }
    }
}