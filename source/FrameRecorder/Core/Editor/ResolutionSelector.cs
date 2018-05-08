using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Recorder
{
    public static class ResolutionSelector
    {
        static readonly string[] s_MaskedNames;
        static readonly Dictionary<ImageResolution, int> s_ImageDimensionToIndex = new Dictionary<ImageResolution, int>();

        static ResolutionSelector()
        {
            s_MaskedNames = EnumHelper.ClipOutEnumNames<ImageResolution>((int)ImageResolution.Window, (int)ImageResolution.x4320p_8K, ToLabel);

            var values = Enum.GetValues(typeof(ImageResolution));
            for (int i = 0; i < values.Length; ++i)
                s_ImageDimensionToIndex[(ImageResolution)values.GetValue(i)] = i;
        }

        public static int Popup(string label, ImageResolution max, int intValue)
        {              
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var index = EnumHelper.GetClippedIndexFromEnumValue<ImageResolution>(intValue, (int)ImageResolution.Window, (int)max);
                index = EditorGUILayout.Popup(label, index, s_MaskedNames.Take(s_ImageDimensionToIndex[max] + 1).ToArray());

                if (check.changed)
                    intValue = EnumHelper.GetEnumValueFromClippedIndex<ImageResolution>(index, (int)ImageResolution.Window, (int)max);

                if (intValue > (int)max)
                    intValue = (int)max;
            }

            return intValue;
        }

        static string ToLabel(ImageResolution value)
        {
            switch (value)
            {
                case ImageResolution.x4320p_8K:
                    return "8K - 4320p";
                case ImageResolution.x2880p_5K:
                    return "5K - 2880p";
                case ImageResolution.x2160p_4K:
                    return "4K - 2160p";
                case ImageResolution.x1440p_QHD:
                    return "QHD - 1440p";
                case ImageResolution.x1080p_FHD:
                    return "FHD - 1080p";
                case ImageResolution.x720p_HD:
                    return "HD - 720p";
                case ImageResolution.x480p:
                    return "SD - 480p";
                case ImageResolution.x240p:
                    return "240p";
                case ImageResolution.Window:
                    return "Match Window Size";
                default:
                    return "unknown";
            }
        }      
    }
}