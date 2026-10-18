using UnityEngine;
using System.Collections.Generic;

public static class BlueprintBootstrap
{
    public static BlueprintDatabase blueprintDatabase;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeSettings()
    {
        blueprintDatabase = Resources.Load<BlueprintDatabase>("BlueprintDatabase");

        if (blueprintDatabase != null)
        {
            blueprintDatabase.Init();
        }
        else
        {
            Debug.LogError("BlueprintDatabase asset missing from Resources folder!");
        }
    }
    public static T GetBlueprint<T>(string id) where T : class, IBlueprint
    {
        return blueprintDatabase.GetBlueprint<T>(id);
    }
}
