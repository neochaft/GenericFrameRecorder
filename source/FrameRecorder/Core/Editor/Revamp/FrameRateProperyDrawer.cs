using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(FrameRateType))]
    class FrameRateProperyDrawer : EnumProperyDrawer<FrameRateType>
    {
        protected override string ToLabel(FrameRateType value)
        {
            switch (value)
            {
                case FrameRateType.FR_23:
                    return "23.97";
                case FrameRateType.FR_24:
                    return "Film (24)";
                case FrameRateType.FR_25:
                    return "PAL (25)";
                case FrameRateType.FR_29:
                    return "NTSC (29.97)";
                case FrameRateType.FR_30:
                    return "30";
                case FrameRateType.FR_50:
                    return "50";
                case FrameRateType.FR_59:
                    return "59.94" ;
                case FrameRateType.FR_60:
                    return "60";
                case FrameRateType.FR_CUSTOM:
                    return "Custom";
                    
                default:
                    return "unknown";
            }
        }       
    }
    
    [CustomPropertyDrawer(typeof(ImageAspect))]
    class ImageAspectProperyDrawer : EnumProperyDrawer<ImageAspect>
    {
        protected override string ToLabel(ImageAspect value)
        {
            switch (value)
            {
                case ImageAspect.x16_9:
                    return "16 x 9";
                case ImageAspect.x16_10:
                    return "16 x 10";
                case ImageAspect.x19_10:
                    return "19 x 10";
                case ImageAspect.x5_4:
                    return "5 x 4";
                case ImageAspect.x4_3:
                    return "4 x 3";
                default:
                    return "unknown";
            }
        }       
    }
}