using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(FrameRate))]
    class FrameRateProperyDrawer : EnumProperyDrawer<FrameRate>
    {
        protected override string ToLabel(FrameRate value)
        {
            switch (value)
            {
                case FrameRate.FR_23:
                    return "23.97";
                case FrameRate.FR_24:
                    return "Film (24)";
                case FrameRate.FR_25:
                    return "PAL (25)";
                case FrameRate.FR_29:
                    return "NTSC (29.97)";
                case FrameRate.FR_30:
                    return "30";
                case FrameRate.FR_50:
                    return "50";
                case FrameRate.FR_59:
                    return "59.94" ;
                case FrameRate.FR_60:
                    return "60";
                case FrameRate.FR_CUSTOM:
                    return "Custom";
                    
                default:
                    return "unknown";
            }
        }       
    }
}