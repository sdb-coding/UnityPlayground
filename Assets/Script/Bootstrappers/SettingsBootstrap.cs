using UnityEngine;
public static class SettingsBootstrap
{
    public static SettingsDatabase settingsData { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeSettings()
    {
        settingsData = Resources.Load<SettingsDatabase>("SettingsData");

        if (settingsData != null)
        {
            settingsData.Init();
        }
        else
        {
            Debug.LogError("SettingsData asset missing from Resources folder!");
        }
    }
}