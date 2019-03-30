using UnityEngine;

namespace SmartHotelMR
{
    public static class Extensions
    {
        #region UnityEngine.Object

        public static void DontDestroyOnLoad(this Object target)
        {
#if UNITY_EDITOR // Skip Don't Destroy On Load when editor isn't playing so test runner passes.
            if (UnityEditor.EditorApplication.isPlaying)
#endif
                Object.DontDestroyOnLoad(target);
        }

        #endregion
    }
}