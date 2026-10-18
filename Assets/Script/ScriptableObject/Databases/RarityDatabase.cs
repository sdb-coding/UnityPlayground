using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using System.Threading.Tasks;

[System.Serializable]
public class RarityConfig
{
    public Rarity rarity;
    public LocalizedString displayName;
    public Color mainColor = Color.white;
    public Color textColor = Color.gray;
}
[CreateAssetMenu(fileName = "RarityDatabase", menuName = "Data/Databases/RarityDatabase")]
public class RarityDatabase : ScriptableObject
{
    [SerializeField] private List<RarityConfig> rarityConfigs;

    private Dictionary<Rarity, RarityConfig> lookupTable;

    public void Init()
    {
        lookupTable = new Dictionary<Rarity, RarityConfig>();
        foreach (var config in rarityConfigs)
        {
            if (!lookupTable.ContainsKey(config.rarity))
            {
                lookupTable.Add(config.rarity, config);
            }
        }
    }

    public RarityConfig GetConfig(Rarity rarity)
    {
        if (lookupTable.TryGetValue(rarity, out var config))
        {
            return config;
        }

        Debug.LogError($"[RarityData] No config found for rarity: {rarity}");
        return null;
    }
}
public static class RarityExtensions
{
    public static RarityConfig GetConfig(this Rarity rarity)
    {
        return RarityBootstrap.Get(rarity);
    }
    public static Color GetColor(this Rarity rarity)
    {
        var config = rarity.GetConfig();
        return config != null ? config.mainColor : Color.white;
    }

    public static async Task<string> GetLocalizedNameAsync(this Rarity rarity)
    {
        var config = rarity.GetConfig();
        if (config == null)
            return null;
        return await config.displayName.GetLocalizedStringAsync().Task;
    }
}
