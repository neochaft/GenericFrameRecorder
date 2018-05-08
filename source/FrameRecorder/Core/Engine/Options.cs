using UnityEditor;

namespace Recorder
{
    public static class Options
    {   
        static bool s_debugMode;

        public static bool debugMode
        {
            get { return s_debugMode; }
            private set
            {
                EditorPrefs.SetBool(s_DebugModeMenuItem, value);
                s_debugMode = value;
            }
        }
    
        const string s_DebugModeMenuItem = "Tools/Recorder/Debug mode";
        const string s_ShowLegacyModeMenuItem = "Tools/Recorder/Show Legacy Recorders";
        
        public static bool showLegacyRecorders { get; private set; }

        static Options()
        {
            debugMode = EditorPrefs.GetBool(s_DebugModeMenuItem, false);
            showLegacyRecorders = EditorPrefs.GetBool(s_ShowLegacyModeMenuItem, false);

            // Delaying until first editor tick so that the menu will be populated before setting check state, and  re-apply correct action
            EditorApplication.delayCall += UpdateMenus;
        }

        static void UpdateMenus()
        {
            Menu.SetChecked(s_DebugModeMenuItem, debugMode);
            Menu.SetChecked(s_ShowLegacyModeMenuItem, showLegacyRecorders);
        }

        [MenuItem(s_DebugModeMenuItem, false, int.MaxValue)]
        static void ToggleDebugMode()
        {
            var value = !debugMode;
            Menu.SetChecked(s_DebugModeMenuItem, value);
            debugMode = value;
        }

        [MenuItem(s_ShowLegacyModeMenuItem, false, int.MaxValue)]
        static void ToggleShowLegacyRecorders()
        {
            var value = !showLegacyRecorders;
            Menu.SetChecked(s_ShowLegacyModeMenuItem, value);
            showLegacyRecorders = value;
        }
    }
}