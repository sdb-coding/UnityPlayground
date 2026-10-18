using UnityEngine;

public class LoadGameModeUIButton : MonoBehaviour
{
    [SerializeField] private SceneReference targetScene;

    public GameBlueprint gameMode;
    private void Awake()
    {
        var button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    void OnClick()
    {
        if (Services.SceneSystem.SceneLoadingManager.PrepLoadPayload(gameMode))
            Services.SceneSystem.SceneLoadingManager.LoadSceneAsync(targetScene.SceneId, UnityEngine.SceneManagement.LoadSceneMode.Single);
        //TODO: Give feedback based on success
    }
}
