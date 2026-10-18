using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SceneDatabase", menuName = "Data/Databases/SceneDatabase")]
public class SceneDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string sceneId;       // Unique GUID saved in game state/logic
        public string scenePath;     // Runtime path (e.g., "Assets/_Project/Scenes/Gameplay.unity")

#if UNITY_EDITOR
        public SceneAsset sceneAsset;// Editor-only reference for easy dragging/dropping
#endif
    }

    [SerializeField] private List<Entry> scenes = new List<Entry>();
    private Dictionary<string, string> idToPathLookup;

    public void Init()
    {
        idToPathLookup = new Dictionary<string, string>();
        foreach (var entry in scenes)
        {
            if (!string.IsNullOrEmpty(entry.sceneId) && !string.IsNullOrEmpty(entry.scenePath))
            {
                if (!idToPathLookup.ContainsKey(entry.sceneId))
                    idToPathLookup.Add(entry.sceneId, entry.scenePath);
            }
        }
    }

    public string GetScenePath(string sceneId)
    {
        if (idToPathLookup == null) Init();
        if (idToPathLookup.TryGetValue(sceneId, out var path))
            return path;
        return null;
    }
    public string GetSceneID(string scenePath)
    {
        if (idToPathLookup == null) Init();
        foreach (var item in idToPathLookup)
        {
            if (item.Value.Equals(scenePath))
                return item.Key;
        }
        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Automatically registers a SceneAsset, returns its unique ID.
    /// </summary>
    public string GetOrRegisterScene(SceneAsset sceneAsset)
    {
        if (sceneAsset == null) return string.Empty;

        string path = AssetDatabase.GetAssetPath(sceneAsset);

        // Check if already exists
        foreach (var item in scenes)
        {
            if (item.scenePath == path)
            {
                return item.sceneId;
            }
        }

        // Add new entry if it doesn't exist yet
        string newId = System.Guid.NewGuid().ToString();
        var newEntry = new Entry
        {
            sceneId = newId,
            scenePath = path,
            sceneAsset = sceneAsset
        };

        scenes.Add(newEntry);
        EditorUtility.SetDirty(this);
        Init();

        return newId;
    }

    private void OnValidate()
    {
        // Keeps paths and IDs synchronized if assets are moved or modified in the inspector
        for (int i = 0; i < scenes.Count; i++)
        {
            var entry = scenes[i];
            if (entry.sceneAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(entry.sceneAsset);
                if (entry.scenePath != path)
                {
                    entry.scenePath = path;
                }
                if (string.IsNullOrEmpty(entry.sceneId))
                {
                    entry.sceneId = System.Guid.NewGuid().ToString();
                }
                scenes[i] = entry;
            }
        }
        Init();
    }
#endif
}