using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveSettingsDatabase", menuName = "Data/SaveSystem/SaveSettingsData")]
public class SaveSettingsDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string assetId;
        public GameObject prefab;
    }
    public string saveFileName = "DeckManiaSave";
    public string versionNumber = "0.0.0.1";

    [SerializeField] private List<Entry> prefabs;
    private Dictionary<string, GameObject> lookup;

    public void Init()
    {
        lookup = new Dictionary<string, GameObject>();
        foreach (var entry in prefabs)
        {
            if (!lookup.ContainsKey(entry.assetId))
                lookup.Add(entry.assetId, entry.prefab);
        }
    }

    public GameObject GetPrefab(string assetId)
    {
        return lookup.TryGetValue(assetId, out var p) ? p : null;
    }
#if UNITY_EDITOR
    public string ContainsPrefab(GameObject prefab)
    {
        foreach (var item in prefabs)
        {
            if (item.prefab == prefab)
            {
                return item.assetId;
            }
        }
        return null;
    }
    public void AddEntry(Entry entry)
    {
        prefabs.Add(entry);
        UnityEditor.EditorUtility.SetDirty(this);
        Init();
    }
#endif
}