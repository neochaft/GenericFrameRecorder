using System;
using UnityEngine.Recorder;

namespace UnityEditor.Recorder
{
    public class ResolutionSelector
    {
        string[] m_MaskedNames;
        EImageDimension m_MaxRes = EImageDimension.Window;

        public int OnGUI(string label, EImageDimension max, int intValue)
        {
            if (m_MaskedNames == null || max != m_MaxRes)
            {
                m_MaskedNames = EnumHelper.ClipOutEnumNames<EImageDimension>((int)EImageDimension.Window, (int)max, ToLabel);
                m_MaxRes = max;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var index = EnumHelper.GetClippedIndexFromEnumValue<EImageDimension>(intValue, (int)EImageDimension.Window, (int)m_MaxRes);
                index = EditorGUILayout.Popup(label, index, m_MaskedNames);

                if (check.changed)
                    intValue = EnumHelper.GetEnumValueFromClippedIndex<EImageDimension>(index, (int)EImageDimension.Window, (int)m_MaxRes);

                if (intValue > (int)m_MaxRes)
                    intValue = (int)m_MaxRes;
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