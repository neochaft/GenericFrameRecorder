using System;

namespace UnityEngine.Recorder
{
    /// <summary>
    /// What is this: Provides the information needed to register Recorder classes with the RecorderInventory.
    /// Motivation  : Dynamically discover Recorder classes and provide a classification system and link between the recorder classes and their Settings classes.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RecorderAttribute : Attribute
    {
        public readonly Type settings;
        public readonly string displayName;
        public readonly string iconName;

        public RecorderAttribute(Type settingsType, string displayName)
        {
            this.settings = settingsType;
            this.displayName = displayName;
        }
        
        public RecorderAttribute(Type settingsType, string displayName, string iconName)
        {
            this.iconName = iconName;
            this.settings = settingsType;
            this.displayName = displayName;
        }
    }
}
