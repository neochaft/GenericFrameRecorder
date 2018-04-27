using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Recorder.Input
{
    public abstract class ImageInputSettings : RecorderInputSetting
    {
        public ImageDimension maxSupportedSize { get; set; } // dynamic & contextual: do not save
        public ImageDimension outputSize = ImageDimension.x720p_HD;
        public ImageAspect aspectRatio = ImageAspect.x16_9;
        public bool forceEvenSize;

        protected ImageInputSettings()
        {
            maxSupportedSize = ImageDimension.x4320p_8K;
        }
        
        public override bool ValidityCheck(List<string> errors)
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