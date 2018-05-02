using System;
using System.IO;
using UnityEngine;
using UnityEngine.Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    [Obsolete("'Legacy/PNG' is obsolete, concider using 'Image Sequence' instead", false)]
    [Recorder(typeof(PNGRecorderSettings), "Legacy/PNG" )]
    public class PNGRecorder : GenericRecorder<PNGRecorderSettings>
    {
        fcAPI.fcPngContext m_ctx;
        
        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            m_Settings.fileNameGenerator.path.CreateDirectory();

            m_ctx = fcAPI.fcPngCreateContext(ref m_Settings.m_PngEncoderSettings);
            return m_ctx;
        }

        public override void EndRecording(RecordingSession session)
        {
            m_ctx.Release();
            base.EndRecording(session);
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");

            var input = (BaseRenderTextureInput)m_Inputs[0];
            var frame = input.outputRT;
            var path = m_Settings.fileNameGenerator.BuildFullPath(session);

            fcAPI.fcLock(frame, (data, fmt) =>
            {
                fcAPI.fcPngExportPixels(m_ctx, path, data, frame.width, frame.height, fmt, 0);
            });
        }

    }
}
