using System;
using System.IO;
using UnityEngine;
using UnityEngine.Recorder;

namespace UTJ.FrameCapturer.Recorders
{
    [Recorder(typeof(GIFRecorderSettings),"Video", "UTJ/GIF" )]
    public class GIFRecorder : GenericRecorder<GIFRecorderSettings>
    {
        fcAPI.fcGifContext m_ctx;
        fcAPI.fcStream m_stream;
        
        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }
            m_Settings.fileNameGenerator.path.CreateDirectory();

            return true;
        }

        public override void EndRecording(RecordingSession session)
        {
            m_ctx.Release();
            m_stream.Release();
            base.EndRecording(session);
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");

            var input = (BaseRenderTextureInput)m_Inputs[0];
            var frame = input.outputRT;

            if(!m_ctx)
            {
                var gifSettings = m_Settings.m_GifEncoderSettings;
                gifSettings.width = frame.width;
                gifSettings.height = frame.height;
                m_ctx = fcAPI.fcGifCreateContext(ref gifSettings);
                var path = m_Settings.fileNameGenerator.BuildFullPath(session);
                m_stream = fcAPI.fcCreateFileStream(path);
                fcAPI.fcGifAddOutputStream(m_ctx, m_stream);
            }

            fcAPI.fcLock(frame, TextureFormat.RGB24, (data, fmt) =>
            {
                fcAPI.fcGifAddFramePixels(m_ctx, data, fmt, session.recorderTime);
            });
        }

    }
}
