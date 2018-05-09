using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Recorder.Input
{
    [DisplayName("Targeted Camera(s)")]
    [Serializable]
    public class CBRenderTextureInputSettings : ImageInputSettings
    {
        public ImageSource source = ImageSource.ActiveCameras;
        public string cameraTag;
        public bool flipFinalOutput;
        public bool allowTransparency;// { get; set; }
        public bool captureUI;

        public override Type inputType
        {
            get { return typeof(CBRenderTextureInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            if (source == ImageSource.TaggedCamera && string.IsNullOrEmpty(cameraTag))
            {
                ok = false;
                errors.Add("Missing tag for camera selection");
            }

            return ok;
        }
    }
}
