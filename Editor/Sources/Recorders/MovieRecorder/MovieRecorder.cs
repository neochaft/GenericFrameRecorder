using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder.Input;
using UnityEditor.Media;

namespace UnityEditor.Recorder
{
    class MovieRecorder : GenericRecorder<MovieRecorderSettings>
    {
        MediaEncoder m_Encoder;
        Texture2D m_ReadBackTexture;

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session))
                return false;

            try
            {
                m_Settings.fileNameGenerator.CreateDirectory(session);
            }
            catch (Exception)
            {
                Debug.LogError(string.Format( "Movie recorder output directory \"{0}\" could not be created.", m_Settings.fileNameGenerator.BuildAbsolutePath(session)));
                return false;
            }

            int width;
            int height;
            if (m_Inputs[0] is ScreenCaptureInput)
            {
                var input = (ScreenCaptureInput)m_Inputs[0];
                width = input.outputWidth;
                height = input.outputHeight;
            }
            else
            {
                var input = (BaseRenderTextureInput)m_Inputs[0];
                if (input == null)
                {
                    if (Options.debugMode)
                        Debug.Log("MediaRecorder could not find input.");
                    return false;
                }
                width = input.outputWidth;
                height = input.outputHeight;
            }

            if (width <= 0 || height <= 0)
            {
                if (Options.debugMode)
                    Debug.Log(string.Format(
                        "MovieRecorder got invalid input resolution {0} x {1}.", width, height));
                return false;
            }

            if (width > 4096 || height > 2160 && m_Settings.outputFormat == VideoRecorderOutputFormat.MP4)
            {
                Debug.LogError("Mp4 format does not support requested resolution.");
            }

            var cbRenderTextureInput = m_Inputs[0] as CBRenderTextureInput;

            bool includeAlphaFromTexture = cbRenderTextureInput != null && cbRenderTextureInput.cbSettings.allowTransparency;
            if (includeAlphaFromTexture && m_Settings.outputFormat == VideoRecorderOutputFormat.MP4)
            {
                Debug.LogWarning("Mp4 format does not support alpha.");
                includeAlphaFromTexture = false;
            }

            var videoAttrs = new VideoTrackAttributes()
            {
                frameRate = RationalFromDouble(session.settings.frameRate),
                width = (uint)width,
                height = (uint)height,
                includeAlpha = includeAlphaFromTexture,
                bitRateMode = m_Settings.videoBitRateMode
            };

            if (Options.debugMode)
                Debug.Log(
                    string.Format(
                        "MovieRecorder starting to write video {0}x{1}@[{2}/{3}] fps into {4}",
                        width, height, videoAttrs.frameRate.numerator,
                        videoAttrs.frameRate.denominator, m_Settings.fileNameGenerator.BuildAbsolutePath(session)));

            var audioInput = (AudioInput)m_Inputs[1];
            var audioAttrsList = new List<AudioTrackAttributes>();
            var audioAttrs = new AudioTrackAttributes
                {
                    sampleRate = new MediaRational
                    {
                        numerator = audioInput.sampleRate,
                        denominator = 1
                    },
                    channelCount = audioInput.channelCount,
                    language = ""
                };
            
            audioAttrsList.Add(audioAttrs);

            if (Options.debugMode)
                Debug.Log( string.Format( "MovieRecorder starting to write audio {0}ch @ {1}Hz", audioAttrs.channelCount, audioAttrs.sampleRate.numerator));

            try
            {
                var path =  m_Settings.fileNameGenerator.BuildAbsolutePath(session);

                m_Encoder = new MediaEncoder( path, videoAttrs, audioAttrsList.ToArray() );
                return true;
            }
            catch
            {
                if (Options.debugMode)
                    Debug.LogError("MovieRecorder unable to create MovieEncoder.");
            }

            return false;
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 2)
                throw new Exception("Unsupported number of sources");

            if (m_Inputs[0] is ScreenCaptureInput)
            {
                var input = (ScreenCaptureInput)m_Inputs[0];
                m_Encoder.AddFrame(input.image);
            }
            else
            {
                var input = (BaseRenderTextureInput)m_Inputs[0];
                var width = input.outputWidth;
                var height = input.outputHeight;

                if (!m_ReadBackTexture)
                    m_ReadBackTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                var backupActive = RenderTexture.active;
                RenderTexture.active = input.outputRT;
                m_ReadBackTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                m_Encoder.AddFrame(m_ReadBackTexture);
                RenderTexture.active = backupActive;
            }

            var audioInput = (AudioInput)m_Inputs[1];
            if (!audioInput.audioSettings.preserveAudio)
                return;

            m_Encoder.AddSamples(audioInput.mainBuffer);
        }

        public override void EndRecording(RecordingSession session)
        {
            base.EndRecording(session);
            if (m_Encoder != null)
            {
                m_Encoder.Dispose();
                m_Encoder = null;
            }

            // When adding a file to Unity's assets directory, trigger a refresh so it is detected.
            if (m_Settings.fileNameGenerator.root == OutputPath.Root.AssetsFolder || m_Settings.fileNameGenerator.root == OutputPath.Root.StreamingAssets)
                AssetDatabase.Refresh();
        }

        // https://stackoverflow.com/questions/26643695/converting-decimal-to-fraction-c
        static long GreatestCommonDivisor(long a, long b)
        {
            if (a == 0)
                return b;

            if (b == 0)
                return a;

            return (a < b) ? GreatestCommonDivisor(a, b % a) : GreatestCommonDivisor(b, a % b);
        }

        static MediaRational RationalFromDouble(double value)
        {
            double integral = Math.Floor(value);
            double frac = value - integral;

            const long precision = 10000000;

            long gcd = GreatestCommonDivisor((long)Math.Round(frac * precision), precision);
            long denom = precision / gcd;

            return new MediaRational()
            {
                numerator = (int)((long)integral * denom + ((long)Math.Round(frac * (double)precision)) / gcd),
                denominator = (int)denom
            };
        }
    }
}