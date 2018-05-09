using System.Collections.Generic;

namespace Recorder.Input
{
    public abstract class ImageInputSettings : RecorderInputSetting
    {
        public ImageResolution maxSupportedSize { get; set; } // dynamic & contextual: do not save
        public ImageResolution outputResolution = ImageResolution.x720p_HD;
        public ImageAspect aspectRatio = ImageAspect.x16_9;
        public bool forceEvenSize;

        protected ImageInputSettings()
        {
            maxSupportedSize = ImageResolution.x4320p_8K;
        }
        
        public override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (outputResolution > maxSupportedSize)
            {
                ok = false;
                errors.Add("Output size exceeds maximum supported size: " + (int)maxSupportedSize );
            }

            return ok;
        }
    }
}