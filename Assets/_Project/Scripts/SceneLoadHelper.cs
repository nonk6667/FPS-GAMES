using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using System.IO;
using UnityEditor.SceneManagement;
#endif

public static class SceneLoadHelper
{
    private const string ProjectSceneFolder = "Assets/_Project/Scenes";

    public static bool LoadScene(string sceneName, string context)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"{context}: Scene name is empty.");
            return false;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"{context}: Loading scene '{sceneName}'.");
            SceneManager.LoadScene(sceneName);
            return true;
        }

#if UNITY_EDITOR
        string scenePath = $"{ProjectSceneFolder}/{sceneName}.unity";

        if (File.Exists(scenePath))
        {
            Debug.Log($"{context}: Loading scene '{sceneName}' from path '{scenePath}'.");
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return true;
        }
#endif

        Debug.LogError($"{context}: Scene '{sceneName}' is not in Build Settings and could not be found in {ProjectSceneFolder}.");
        return false;
    }
}
