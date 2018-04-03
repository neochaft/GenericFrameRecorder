using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEngine.Recorder.Input
{
    public enum ESuperSamplingCount
    {
        X1 = 1,
        X2 = 2,
        X4 = 4,
        X8 = 8,
        X16 = 16,
    }

    [DisplayName("Texture Sampling")]
    [Serializable]
    public class RenderTextureSamplerSettings : ImageInputSettings
    {
        public EImageSource source = EImageSource.ActiveCameras;
        public EImageDimension renderSize = EImageDimension.x720p_HD;
        public ESuperSamplingCount superSampling = ESuperSamplingCount.X1;
        public float superKernelPower = 16f;
        public float superKernelScale = 1f;
        public string cameraTag;
        public ColorSpace colorSpace = ColorSpace.Gamma;
        public bool flipFinalOutput = false;

        public override Type inputType
        {
            get { return typeof(RenderTextureSampler); }
        }

        public override bool ValidityCheck( List<string> errors )
        {
            return true;
        }
    }
}