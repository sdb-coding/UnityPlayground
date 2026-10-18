using UnityEngine;
using System.Collections.Generic;

public class RarityBootstrap : MonoBehaviour
{
    public static RarityDatabase rarityData;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeSettings()
    {
        rarityData = Resources.Load<RarityDatabase>("RarityData");

        if (rarityData != null)
        {
            rarityData.Init();
        }
        else
        {
            Debug.LogError("RarityData asset missing from Resources folder!");
        }
    }

    public static RarityConfig Get(Rarity rarity)
    {
        return rarityData.GetConfig(rarity);
    }
}
