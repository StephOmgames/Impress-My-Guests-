using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ImpressMyGuests.Core
{
    /// <summary>
    /// Utility class for loading scenes with an optional loading screen.
    /// </summary>
    public static class SceneLoader
    {
        public static event Action<string> OnSceneLoadStarted;
        public static event Action<string> OnSceneLoadCompleted;

        /// <summary>Loads a scene by name synchronously.</summary>
        public static void LoadScene(string sceneName)
        {
            OnSceneLoadStarted?.Invoke(sceneName);
            SceneManager.LoadScene(sceneName);
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        /// <summary>Loads a scene additively (e.g. for overlays).</summary>
        public static void LoadSceneAdditive(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        /// <summary>Reloads the currently active scene.</summary>
        public static void ReloadCurrentScene()
        {
            string current = SceneManager.GetActiveScene().name;
            LoadScene(current);
        }
    }
}
