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
    public enum FrameRate
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
        public static float ToFloat(FrameRate frameRate, float customValue)
        {
            switch (frameRate)
            {
                case FrameRate.FR_CUSTOM:
                    return customValue;
                case FrameRate.FR_23:
                    return 24 * 1000 / 1001f;
                case FrameRate.FR_24:
                    return 24;
                case FrameRate.FR_25:
                    return 25;
                case FrameRate.FR_29:
                    return 30 * 1000 / 1001f;
                case FrameRate.FR_30:
                    return 30;
                case FrameRate.FR_50:
                    return 50;
                case FrameRate.FR_59:
                    return 60 * 1000 / 1001f;
                case FrameRate.FR_60:
                    return 60;
                default:
                    throw new ArgumentOutOfRangeException("frameRate", frameRate, null);
            }
        }

        public static string ToLable(FrameRate frameRate)
        {
            switch (frameRate)
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
                default:
                    return "Custom";
            }
        }       
    }
}
