using Services.SceneSystem;
using UnityEngine;

public class BootstrapSceneRunner : MonoBehaviour
{
    public SceneReference defaultFirstScene;
#if UNITY_EDITOR
    public const string BootstrapSceneStartSceneID = "BootstrapSceneRunner_BootSceneId";
    void Start()
    {
        var editorPrefValue = UnityEditor.EditorPrefs.GetString(BootstrapSceneStartSceneID);
        if (string.IsNullOrEmpty(editorPrefValue))
            SceneLoadingManager.LoadSceneAsync(defaultFirstScene.SceneId, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        else
            SceneLoadingManager.LoadSceneAsync(editorPrefValue, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
#else
    void Start()
    {
        SceneLoadingManager.LoadSceneAsync(defaultFirstScene.SceneId, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
#endif
}
