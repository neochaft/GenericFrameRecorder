using Object = UnityEngine.Object;

namespace UnityEditor.Recorder
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
            
            if (EditorApplication.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj, allowDestroyingAssets);
        }

        public static bool IsPlaying()
        {
            return EditorApplication.isPlaying;
        }
    }
}
