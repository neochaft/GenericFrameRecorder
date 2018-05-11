namespace UnityEditor.Recorder
{
    public static class Options
    {   
        static bool s_debugMode;

        public static bool debugMode
        {
            get { return s_debugMode; }
            private set
            {
                EditorPrefs.SetBool(k_DebugModeMenuItem, value);
                s_debugMode = value;
            }
        }
    
        const string k_DebugModeMenuItem = "Recorder/Options/Debug mode";
        const string k_ShowLegacyModeMenuItem = "Recorder/Options/Show Legacy Recorders";
        
        public static bool showLegacyRecorders { get; private set; }

        static Options()
        {
            debugMode = EditorPrefs.GetBool(k_DebugModeMenuItem, false);
            showLegacyRecorders = EditorPrefs.GetBool(k_ShowLegacyModeMenuItem, false);

            // Delaying until first editor tick so that the menu will be populated before setting check state, and  re-apply correct action
            EditorApplication.delayCall += UpdateMenus;
        }

        static void UpdateMenus()
        {
            Menu.SetChecked(k_DebugModeMenuItem, debugMode);
            Menu.SetChecked(k_ShowLegacyModeMenuItem, showLegacyRecorders);
        }

        [MenuItem(k_DebugModeMenuItem, false, int.MaxValue)]
        static void ToggleDebugMode()
        {
            var value = !debugMode;
            Menu.SetChecked(k_DebugModeMenuItem, value);
            debugMode = value;
        }

        [MenuItem(k_ShowLegacyModeMenuItem, false, int.MaxValue)]
        static void ToggleShowLegacyRecorders()
        {
            var value = !showLegacyRecorders;
            Menu.SetChecked(k_ShowLegacyModeMenuItem, value);
            showLegacyRecorders = value;
        }
    }
}