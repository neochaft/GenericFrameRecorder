using Object = UnityEngine.Object;

namespace Recorder
{

    /// <summary>
    /// What is this: 
    /// Motivation  : 
    /// Notes: 
    /// </summary>    
    public static class UnityHelpers
    {
        public static void Destroy(Object obj, bool allowDestroyingAssets = false)
        {
            if (obj == null)
                return;
            
            if (UnityEditor.EditorApplication.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj, allowDestroyingAssets);
        }

        public static bool IsPlaying()
        {
            return UnityEditor.EditorApplication.isPlaying;
        }
    }
}
