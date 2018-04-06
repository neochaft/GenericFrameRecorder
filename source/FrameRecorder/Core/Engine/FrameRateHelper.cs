using System;

namespace UnityEngine.Recorder
{

    /// <summary>
    /// What is this: Standard industry frame rates.
    /// Motivation  : Some framerates are not correctly expressible as floats du to precision, so
    ///               so having an enum with the standard frame rates used in industry, allows us
    ///               to correctly carry precision in the settings. Precision loss is then the fault
    ///               of the components further down the line.
    /// </summary>
    //[Flags]
    public enum FrameRateType
    {
        FR_23, // 24 * 1000 / 1001
        FR_24,
        FR_25,
        FR_29, // 30 * 1000 / 1001,
        FR_30,
        FR_50,
        FR_59, // 60 * 1000 / 1001,
        FR_60,
        FR_CUSTOM,
    }

    /// <summary>
    /// What is this: Utility class that converts  EFrameRate to text and to float 
    /// Motivation  : since the enum is expressed as integers, need something to provide associated float values
    ///               and also provide human readable lables for the UI.
    /// </summary>    
    public static class FrameRateHelper
    {
        public static float ToFloat(FrameRateType frameRateType, float customValue)
        {
            switch (frameRateType)
            {
                case FrameRateType.FR_CUSTOM:
                    return customValue;
                case FrameRateType.FR_23:
                    return 24 * 1000 / 1001f;
                case FrameRateType.FR_24:
                    return 24;
                case FrameRateType.FR_25:
                    return 25;
                case FrameRateType.FR_29:
                    return 30 * 1000 / 1001f;
                case FrameRateType.FR_30:
                    return 30;
                case FrameRateType.FR_50:
                    return 50;
                case FrameRateType.FR_59:
                    return 60 * 1000 / 1001f;
                case FrameRateType.FR_60:
                    return 60;
                default:
                    throw new ArgumentOutOfRangeException("frameRateType", frameRateType, null);
            }
        }

        public static string ToLable(FrameRateType frameRateType)
        {
            switch (frameRateType)
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
                default:
                    return "Custom";
            }
        }       
    }
}
