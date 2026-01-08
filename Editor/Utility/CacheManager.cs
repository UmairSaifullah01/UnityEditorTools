using UnityEditor;
#if UNITY_2019_3_OR_NEWER
using UnityEditor.Compilation;
#endif
using UnityEngine;

namespace THEBADDEST.EditorTools
{
    /// <summary>
    /// Manages cache clearing for performance optimizations.
    /// Automatically clears caches when assemblies reload.
    /// </summary>
    [InitializeOnLoad]
    public static class CacheManager
    {
        static CacheManager()
        {
            // Clear all caches when domain reloads (assembly compilation, script changes, etc.)
            ClearAllCaches();
            
            #if UNITY_2019_3_OR_NEWER
            // Use newer API if available
            CompilationPipeline.compilationFinished += (obj) => ClearAllCaches();
            #endif
        }

        /// <summary>
        /// Clears all performance-related caches.
        /// </summary>
        public static void ClearAllCaches()
        {
            PropertyUtility.ClearCache();
            ReflectionUtility.ClearCache();
            
            // Clear scene cache by invalidating it
            ScenePropertyDrawer.InvalidateSceneCache();
        }
    }
}

