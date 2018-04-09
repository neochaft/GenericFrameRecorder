using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEngine.Recorder.Input
{
    [DisplayName("Render Texture Asset")]
    [Serializable]
    public class RenderTextureInputSettings : ImageInputSettings
    {
        public RenderTexture sourceRTxtr;
        

        public override Type inputType
        {
            get { return typeof(RenderTextureInput); }
        }

        public override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (sourceRTxtr == null)
            {
                ok = false;
                errors.Add("Missing source render texture object/asset.");
            }

            return ok;
        }
    }
}
