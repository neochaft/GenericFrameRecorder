using UnityEditor;

namespace Recorder
{
    [CustomPropertyDrawer(typeof(FrameResolution))]
    class FrameResolutionProperyDrawer : EnumProperyDrawer<FrameResolution>
    {
        protected override string ToLabel(FrameResolution value)
        {
            switch (value)
            {
                case FrameResolution.X240P:
                    return "240p";
                case FrameResolution.X480P:
                    return "480p";
                case FrameResolution.X720P_HD:
                    return "720p (HD)";
                case FrameResolution.X1080P_FHD:
                    return "1080p (FHD)";
                case FrameResolution.X1440P_QHD:
                    return "1440p (QHD)";
                case FrameResolution.X2160P_4K:
                    return "2160p (4K)";
                case FrameResolution.X2880P_5K:
                    return "2880p (5K)";
                case FrameResolution.X4320P_8K:
                    return "4320p (8K)";
                case FrameResolution.Custom:
                    return "Custom";
                default:
                    return "unknown";
            }
        }       
    }
}