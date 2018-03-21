using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Audio")]
    public class AudioInputSettings : RecorderInputSetting
    {
        public bool preserveAudio = true;

#if RECORD_AUDIO_MIXERS
        [System.Serializable]
        public struct MixerGroupRecorderListItem
        {
            [SerializeField]
            public AudioMixerGroup m_MixerGroup;
            
            [SerializeField]
            public bool m_Isolate;
        }
        public MixerGroupRecorderListItem[] m_AudioMixerGroups;
#endif

        public override Type inputType
        {
            get { return typeof(AudioInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            return true;
        }
    }
}