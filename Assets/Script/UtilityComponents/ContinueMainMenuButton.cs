using UnityEngine;
using UnityEngine.UI;

public class ContinueMainMenuButton : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }
    void OnEnable()
    {
        gameObject.SetActive(Services.SaveSystem.SaveLoadManager.HasActiveRun());
    }
    void OnClick()
    {
        Services.SaveSystem.SaveLoadManager.LoadActiveRun();
    }
}
