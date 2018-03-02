using System;
using System.Collections.Generic;
using UnityEditor.Experimental.FrameRecorder;
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
    // Main class for a recorder
    public abstract class RecorderBase
    {
        protected RecorderEditor m_RecorderEditor;
    }

    public class MovieRecorder : RecorderBase
    {
        
    }
    
    public class AnimationClipRecorder : RecorderBase
    {
        public AnimationClipRecorder()
        {
            m_RecorderEditor = ScriptableObject.CreateInstance<AnimationRecorderSettingsEditor>();
        }
    }
    
    public class ImageSequenceRecorder : RecorderBase
    {
        
    }
}
