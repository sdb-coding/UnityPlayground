using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Services.SaveSystem
{
    public class AssetModificationProcessorRunner : UnityEditor.AssetModificationProcessor
    {
        private static readonly HashSet<string> PathsToProcess = new HashSet<string>();
        private static bool isProcessingDeferred = false;

        private static string[] OnWillSaveAssets(string[] paths)
        {
            var saveSettingData = Resources.Load<SaveSettingsDatabase>("SaveSettingsData");
            if (saveSettingData == null) return paths;

            saveSettingData.Init();

            foreach (string path in paths)
            {
                if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    PathsToProcess.Add(path);
                }
            }

            if (PathsToProcess.Count > 0 && !isProcessingDeferred)
            {
                isProcessingDeferred = true;
                EditorApplication.delayCall += ProcessDeferredPrefabs;
            }

            return paths;
        }

        private static void ProcessDeferredPrefabs()
        {
            EditorApplication.delayCall -= ProcessDeferredPrefabs;
            isProcessingDeferred = false;

            var saveSettingData = Resources.Load<SaveSettingsDatabase>("SaveSettingsData");
            if (saveSettingData == null)
            {
                PathsToProcess.Clear();
                return;
            }

            bool assetsModified = false;

            foreach (string path in PathsToProcess)
            {
                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabRoot == null) continue;

                SaveEntity targetComponent = prefabRoot.GetComponentInChildren<SaveEntity>();

                if (targetComponent != null)
                {
                    string existingId = saveSettingData.ContainsPrefab(prefabRoot);

                    if (string.IsNullOrEmpty(existingId))
                    {
                        existingId = System.Guid.NewGuid().ToString();

                        saveSettingData.AddEntry(new SaveSettingsDatabase.Entry()
                        {
                            assetId = existingId,
                            prefab = prefabRoot
                        });

                        assetsModified = true;
                    }

                    if (targetComponent.PrefabAssetId != existingId)
                    {
                        Undo.RecordObject(targetComponent, "Update Prefab Asset ID");
                        targetComponent.SetPrefabAssetID(existingId);

                        EditorUtility.SetDirty(targetComponent);
                        EditorUtility.SetDirty(prefabRoot);

                        assetsModified = true;
                    }
                }
            }

            PathsToProcess.Clear();

            if (assetsModified)
            {
                EditorUtility.SetDirty(saveSettingData);
                AssetDatabase.SaveAssets();
            }
        }
    }
}