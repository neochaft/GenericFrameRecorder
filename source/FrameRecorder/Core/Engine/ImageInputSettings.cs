using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Recorder.Input
{
    public abstract class ImageInputSettings : RecorderInputSetting
    {
        public EImageDimension maxSupportedSize { get; set; } // dynamic & contextual: do not save
        public EImageDimension outputSize = EImageDimension.x720p_HD;
        public EImageAspect aspectRatio = EImageAspect.x16_9;
        public bool forceEvenSize;

        protected ImageInputSettings()
        {
            maxSupportedSize = EImageDimension.x4320p_8K;
        }
        
        public override bool ValidityCheck( List<string> errors )
        {
            var ok = true;

            if (outputSize > maxSupportedSize)
            {
                ok = false;
                errors.Add("Output size exceeds maximum supported size: " + (int)maxSupportedSize );
            }

            return ok;
        }
    }
}