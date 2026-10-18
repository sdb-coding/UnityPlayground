using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "BlueprintDatabase", menuName = "Data/Databases/BlueprintDatabase")]
public class BlueprintDatabase : ScriptableObject
{
    [System.Serializable]
    public struct BlueprintRef
    {
        public string id;
        public ScriptableObject blueprintAsset; // Holds any blueprint implementing IBlueprint
    }

    [SerializeField] private List<BlueprintRef> blueprintList = new List<BlueprintRef>();

    private Dictionary<string, IBlueprint> blueprintLookupTable;

    public void Init()
    {
        blueprintLookupTable = new Dictionary<string, IBlueprint>();

        foreach (var blueprintRef in blueprintList)
        {
            if (blueprintRef.blueprintAsset is IBlueprint blueprint)
            {
                if (!string.IsNullOrEmpty(blueprintRef.id) && !blueprintLookupTable.ContainsKey(blueprintRef.id))
                {
                    blueprintLookupTable.Add(blueprintRef.id, blueprint);
                }
            }
        }
    }

    /// <summary>
    /// Generic getter that casts the blueprint to your requested type.
    /// Usage: T blueprint = database.GetBlueprint<PlayingCardBlueprint>("my_card_id");
    /// </summary>
    public T GetBlueprint<T>(string id) where T : class, IBlueprint
    {
        if (blueprintLookupTable != null && blueprintLookupTable.TryGetValue(id, out var blueprint))
        {
            if (blueprint is T typedBlueprint)
            {
                return typedBlueprint;
            }
            Debug.LogWarning($"[BlueprintDatabase] Blueprint with id '{id}' was found, but is not of type {typeof(T).Name}");
        }

        Debug.LogWarning($"[BlueprintDatabase] No blueprint found for id: {id}");
        return null;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BlueprintDatabase))]
    public class BlueprintDatabase_Editor : Editor
    {
        BlueprintDatabase myTarget;
        private void OnEnable()
        {
            myTarget = (BlueprintDatabase)target;
        }
        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Populate list"))
                {
                    myTarget.PopulateList();
                }
            }
            base.OnInspectorGUI();
        }
    }

    [ContextMenu("Populate list")]
    private void PopulateList()
    {
        blueprintList ??= new List<BlueprintRef>();
        blueprintList.Clear();

        // Automatically find all assets in the project that implement IBlueprint
        string[] guids = AssetDatabase.FindAssets($"t:ScriptableObject");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset is IBlueprint blueprint)
            {
                if (string.IsNullOrEmpty(blueprint.ID))
                {
                    blueprint.GenerateID();
                    EditorUtility.SetDirty(asset);
                }

                blueprintList.Add(new BlueprintRef
                {
                    id = blueprint.ID,
                    blueprintAsset = asset
                });
                count++;
            }
        }

        ValidateUniqueIDs();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log($"[BlueprintDatabase] Populated {count} blueprints successfully.");
    }

    [ContextMenu("Validate Unique IDs")]
    public bool ValidateUniqueIDs()
    {
        HashSet<string> seenIDs = new HashSet<string>();
        bool hasDuplicates = hasDuplicates = false;

        foreach (var blueprintRef in blueprintList)
        {
            if (string.IsNullOrEmpty(blueprintRef.id)) continue;

            if (!seenIDs.Add(blueprintRef.id))
            {
                Debug.LogWarning($"[BlueprintDatabase] Duplicate ID found: '{blueprintRef.id}'", blueprintRef.blueprintAsset);
                hasDuplicates = true;
            }
        }

        if (!hasDuplicates)
        {
            Debug.Log("[BlueprintDatabase] All IDs are unique!");
        }

        return !hasDuplicates;
    }
#endif
}
