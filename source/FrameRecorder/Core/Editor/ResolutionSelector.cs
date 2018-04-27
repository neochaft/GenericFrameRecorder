using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public static class ResolutionSelector
    {
        static readonly string[] s_MaskedNames;
        static readonly Dictionary<ImageDimension, int> s_ImageDimensionToIndex = new Dictionary<ImageDimension, int>();

        static ResolutionSelector()
        {
            s_MaskedNames = EnumHelper.ClipOutEnumNames<ImageDimension>((int)ImageDimension.Window, (int)ImageDimension.x4320p_8K, ToLabel);

            var values = Enum.GetValues(typeof(ImageDimension));
            for (int i = 0; i < values.Length; ++i)
                s_ImageDimensionToIndex[(ImageDimension)values.GetValue(i)] = i;
        }

        public static int Popup(string label, ImageDimension max, int intValue)
        {              
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var index = EnumHelper.GetClippedIndexFromEnumValue<ImageDimension>(intValue, (int)ImageDimension.Window, (int)max);
                index = EditorGUILayout.Popup(label, index, s_MaskedNames.Take(s_ImageDimensionToIndex[max] + 1).ToArray());

                if (check.changed)
                    intValue = EnumHelper.GetEnumValueFromClippedIndex<ImageDimension>(index, (int)ImageDimension.Window, (int)max);

                if (intValue > (int)max)
                    intValue = (int)max;
            }

            return intValue;
        }

        static string ToLabel(ImageDimension value)
        {
            switch (value)
            {
                case ImageDimension.x4320p_8K:
                    return "8K - 4320p";
                case ImageDimension.x2880p_5K:
                    return "5K - 2880p";
                case ImageDimension.x2160p_4K:
                    return "4K - 2160p";
                case ImageDimension.x1440p_QHD:
                    return "QHD - 1440p";
                case ImageDimension.x1080p_FHD:
                    return "FHD - 1080p";
                case ImageDimension.x720p_HD:
                    return "HD - 720p";
                case ImageDimension.x480p:
                    return "SD - 480p";
                case ImageDimension.x240p:
                    return "240p";
                case ImageDimension.Window:
                    return "Match Window Size";
                default:
                    return "unknown";
            }
        }      
    }
}