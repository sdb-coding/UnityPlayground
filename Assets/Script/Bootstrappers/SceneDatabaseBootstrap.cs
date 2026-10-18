using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SceneDatabaseBootstrap
{
    public static SceneDatabase sceneDatabase;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeSettings()
    {
        sceneDatabase = Resources.Load<SceneDatabase>("SceneDatabase");

        if (sceneDatabase != null)
        {
            sceneDatabase.Init();
        }
        else
        {
            Debug.LogError("SceneDatabase asset missing from Resources folder!");
        }
    }

    public static string GetScenePath(string guid)
    {
        return sceneDatabase.GetScenePath(guid);
    }
    public static string GetSceneID(string path)
    {
        return sceneDatabase.GetSceneID(path);
    }
}


#if UNITY_EDITOR
public static class SceneDatabaseEditorUtility
{
    private static SceneDatabase cachedDatabase;

    public static string GetOrRegisterScene(SceneAsset sceneAsset)
    {
        if (sceneAsset == null) return string.Empty;

        if (cachedDatabase == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:SceneDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cachedDatabase = AssetDatabase.LoadAssetAtPath<SceneDatabase>(path);
            }
        }

        if (cachedDatabase != null)
        {
            return cachedDatabase.GetOrRegisterScene(sceneAsset);
        }

        Debug.LogWarning("SceneDatabaseUtility: No SceneDatabase asset found in the project!");
        return string.Empty;
    }
    public static string GetScenePath(string sceneID)
    {
        if (cachedDatabase == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:SceneDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cachedDatabase = AssetDatabase.LoadAssetAtPath<SceneDatabase>(path);
            }
        }

        if (cachedDatabase != null)
        {
            return cachedDatabase.GetScenePath(sceneID);
        }
        return null;
    }
}
#endif
