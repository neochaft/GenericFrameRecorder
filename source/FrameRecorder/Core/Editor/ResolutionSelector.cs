using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class ResolutionSelector
    {
        static readonly string[] s_MaskedNames;
        static readonly Dictionary<EImageDimension, int> s_ImageDimensionToIndex = new Dictionary<EImageDimension, int>();

        static ResolutionSelector()
        {
            s_MaskedNames = EnumHelper.ClipOutEnumNames<EImageDimension>((int)EImageDimension.Window, (int)EImageDimension.x4320p_8K, ToLabel);

            var values = Enum.GetValues(typeof(EImageDimension));
            for (int i = 0; i < values.Length; ++i)
                s_ImageDimensionToIndex[(EImageDimension)values.GetValue(i)] = i;
            
        }

        public static int Popup(string label, EImageDimension max, int intValue)
        {              
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var index = EnumHelper.GetClippedIndexFromEnumValue<EImageDimension>(intValue, (int)EImageDimension.Window, (int)max);
                index = EditorGUILayout.Popup(label, index, s_MaskedNames.Take(s_ImageDimensionToIndex[max] + 1).ToArray());

                if (check.changed)
                    intValue = EnumHelper.GetEnumValueFromClippedIndex<EImageDimension>(index, (int)EImageDimension.Window, (int)max);

                if (intValue > (int)max)
                    intValue = (int)max;
            }

            return intValue;
        }

        static string ToLabel(EImageDimension value)
        {
            switch (value)
            {
                case EImageDimension.x4320p_8K:
                    return "8K - 4320p";
                case EImageDimension.x2880p_5K:
                    return "5K - 2880p";
                case EImageDimension.x2160p_4K:
                    return "4K - 2160p";
                case EImageDimension.x1440p_QHD:
                    return "QHD - 1440p";
                case EImageDimension.x1080p_FHD:
                    return "FHD - 1080p";
                case EImageDimension.x720p_HD:
                    return "HD - 720p";
                case EImageDimension.x480p:
                    return "SD - 480p";
                case EImageDimension.x240p:
                    return "240p";
                case EImageDimension.Window:
                    return "Match Window Size";
                default:
                    return "unknown";
            }
        }      
    }
}