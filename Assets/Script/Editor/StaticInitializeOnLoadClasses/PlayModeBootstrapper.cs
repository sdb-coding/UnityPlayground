using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class PlayModeBootstrapper
{
    static PlayModeBootstrapper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string bootSceneId = EditorPrefs.GetString(EditorPrefKeys.BootSceneGuidPref, "1e094d93-60ef-4a06-9060-0ffd55d6e491");
            if (string.IsNullOrEmpty(bootSceneId))
            {
                Debug.LogWarning("PlayModeBootstrapper: No boot scene configured. Use 'Tools > Scene System > Set Current Scene as Boot Scene'.");
                return;
            }
            EditorPrefs.SetString(BootstrapSceneRunner.BootstrapSceneStartSceneID, null); //Reset the followup scene
            string bootstrapScenePath = SceneDatabaseEditorUtility.GetScenePath(bootSceneId);
            if (!string.IsNullOrEmpty(bootstrapScenePath))
            {
                SceneAsset bootSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootstrapScenePath);
                if (bootSceneAsset != null)
                {
                    var currentActiveScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                    if (currentActiveScene.IsValid() && !string.IsNullOrEmpty(currentActiveScene.path) && !currentActiveScene.path.Equals(bootstrapScenePath))
                        EditorPrefs.SetString(BootstrapSceneRunner.BootstrapSceneStartSceneID, SceneDatabaseEditorUtility.GetOrRegisterScene(AssetDatabase.LoadAssetAtPath<SceneAsset>(currentActiveScene.path)));
                    UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = bootSceneAsset;
                }
                else
                {
                    Debug.LogWarning($"PlayModeBootstrapper: SceneAsset could not be loaded at path: {bootstrapScenePath}");
                }
            }
            else
            {
                Debug.LogWarning($"PlayModeBootstrapper: Could not find a scene path for ID: {bootSceneId} in the database.");
            }
        }
    }
    /// <summary>
    /// Helper menu item to register the currently open scene into the database 
    /// and save its GUID as the target boot scene.
    /// </summary>
    [MenuItem("Tools/Scene System/Set Current Scene as Boot Scene")]
    public static void SetCurrentSceneAsBoot()
    {
        string currentScenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
        SceneAsset currentSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(currentScenePath);

        if (currentSceneAsset == null)
        {
            EditorUtility.DisplayDialog("Boot Scene Setup", "Please save your current scene before setting it as the boot scene.", "OK");
            return;
        }

        string sceneId = SceneDatabaseEditorUtility.GetOrRegisterScene(currentSceneAsset);
        EditorPrefs.SetString(EditorPrefKeys.BootSceneGuidPref, sceneId);

        EditorUtility.DisplayDialog("Boot Scene Setup", $"Successfully set '{currentSceneAsset.name}' as the Play Mode Boot Scene! With id: {sceneId}", "OK");
    }
}
