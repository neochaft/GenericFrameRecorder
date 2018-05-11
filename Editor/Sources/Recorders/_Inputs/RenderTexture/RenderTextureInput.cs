using System;

namespace UnityEditor.Recorder.Input
{
    class RenderTextureInput : BaseRenderTextureInput
    {
        RenderTextureInputSettings cbSettings
        {
            get { return (RenderTextureInputSettings)settings; }
        }

        public override void BeginRecording(RecordingSession session)
        {
            if (cbSettings.sourceRTxtr == null)
                throw new Exception("No Render Texture object provided as source");

            outputHeight = cbSettings.sourceRTxtr.height;
            outputWidth = cbSettings.sourceRTxtr.width;
            outputRT = cbSettings.sourceRTxtr;
        }
    }
}