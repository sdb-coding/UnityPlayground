using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.SaveSystem
{
    public class MasterSaveData
    {
        //Future additions could include achivement containers or collection trackers
        public ActiveRun activeRun;
    }
    public class ActiveRun
    {
        public string activeRunSceneID;
        public List<EntitySaveData> entitySaveDatas;
    }
    public class EntitySaveData
    {
        public string saveKey;
        public string prefabAssetId; // Null for static scene objects; filled for dynamic prefabs
        public Dictionary<string, object> componentsState;
    }

    public static class SaveLoadManager
    {
        static SaveSettingsDatabase saveSettingData;
        public static bool SaveInProgress { get; private set; }
        public static bool LoadInProgress { get; private set; }
        private static readonly Dictionary<string, SaveEntity> activeEntities = new();
        private static string SaveFilePath; // => Path.Combine(Application.persistentDataPath, saveSettingData.saveFileName);
        private static string TempSaveFilePath; // => Path.Combine(Application.persistentDataPath, saveSettingData.saveFileName + "_Temp");
        private static string BackupSavePath; // => Path.Combine(Application.persistentDataPath, saveSettingData.saveFileName + "_Backup");

        static MasterSaveData masterSaveData;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeSettings()
        {
            saveSettingData = Resources.Load<SaveSettingsDatabase>("SaveSettingsData");
            SaveFilePath = Path.Combine(Application.persistentDataPath, saveSettingData.saveFileName);
            TempSaveFilePath = Path.Combine(Application.persistentDataPath, saveSettingData.saveFileName + "_Temp");
            BackupSavePath = Path.Combine(Application.persistentDataPath, saveSettingData.saveFileName + "_Backup");

            if (saveSettingData != null)
            {
                saveSettingData.Init();
            }
            else
            {
                Debug.LogError("SettingsData asset missing from Resources folder!");
            }
            LoadMasterSaveData();
        }

        public static void Register(SaveEntity entity)
        {
            if (entity != null)
                activeEntities[entity.SaveKey] = entity;
        }

        public static void Unregister(SaveEntity entity)
        {
            if (entity != null)
                activeEntities.Remove(entity.SaveKey);
        }

        public static async void SaveRun()
        {
            if (SaveInProgress || LoadInProgress)
            {
                Debug.Log("Save or load already in progress, aborting");
                return;
            }
            SaveInProgress = true;

            masterSaveData = GenerateMasterSaveData();
            bool success = await SaveAsync(masterSaveData);
            if (success)
                Debug.Log("Save Complete");
            SaveInProgress = false;
        }
        static MasterSaveData GenerateMasterSaveData()
        {
            MasterSaveData masterSaveData = new MasterSaveData();
            masterSaveData.activeRun = new ActiveRun()
            {
                activeRunSceneID = SceneDatabaseBootstrap.GetSceneID(SceneManager.GetActiveScene().path),
                entitySaveDatas = GenerateEntitySaveData()
            };
            return masterSaveData;
        }
        static List<EntitySaveData> GenerateEntitySaveData()
        {
            var saveList = new List<EntitySaveData>();

            foreach (var (key, entity) in activeEntities)
            {
                var state = entity.GatherState();
                if (state.Count > 0)
                {
                    saveList.Add(new EntitySaveData
                    {
                        saveKey = key,
                        prefabAssetId = entity.PrefabAssetId,
                        componentsState = state
                    });
                }
            }
            return saveList;
        }
        static async Task<bool> SaveAsync(MasterSaveData masterSaveData)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string json = JsonConvert.SerializeObject(masterSaveData, ReflectionSaveEngine._cachedSettings);
                    var succes = SaveStringToFile(json);
                    Debug.Log($"Saved fullsavedata with: {masterSaveData.activeRun.entitySaveDatas.Count} entities.");
                    return succes.Result;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveSystem] Async save failed: {ex.Message}");

                    // Cleanup temp file if stranded
                    if (File.Exists(TempSaveFilePath))
                    {
                        try { File.Delete(TempSaveFilePath); } catch { /* ignore */ }
                    }

                    return false;
                }
            });
        }
        static async Task<bool> SaveStringToFile(string json)
        {
            try
            {
                await File.WriteAllTextAsync(TempSaveFilePath, json);
                if (File.Exists(SaveFilePath))
                {
                    File.Replace(TempSaveFilePath, SaveFilePath, BackupSavePath, ignoreMetadataErrors: true);
                }
                else
                {
                    File.Move(TempSaveFilePath, SaveFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                if (File.Exists(TempSaveFilePath))
                {
                    try { File.Delete(TempSaveFilePath); } catch { /* ignore */ }
                }
                return false;
            }
        }
        static async Task LoadMasterSaveData()
        {
            masterSaveData = await LoadMasterSaveDataAsync();
        }
        public static async void LoadActiveRun()
        {
            if (SaveInProgress || LoadInProgress)
            {
                Debug.Log("Save or load already in progress, aborting");
                return;
            }
            LoadInProgress = true;
            RunGlobalPrepareForLoad();
            await LoadMasterSaveData();

            await RestoreRun(masterSaveData.activeRun);
            LoadInProgress = false;
        }
        static async Task RestoreRun(ActiveRun activeRun)
        {
            if (activeRun == null || activeRun.entitySaveDatas == null)
            {
                Debug.LogError($"[SaveSystem] No data found in load");
                return;
            }
            if (!Services.SceneSystem.SceneLoadingManager.IsSceneLoaded(activeRun.activeRunSceneID))
            {
                var tcs = new TaskCompletionSource<bool>();
                bool loadSuccessful = Services.SceneSystem.SceneLoadingManager.LoadSceneAsync(activeRun.activeRunSceneID, UnityEngine.SceneManagement.LoadSceneMode.Single, () =>
                {
                    tcs.SetResult(true);
                });
                if (!loadSuccessful)
                    return;
                loadSuccessful = await tcs.Task;
            }
            var currentEntities = new List<SaveEntity>(activeEntities.Values);
            foreach (var entity in currentEntities)
            {
                if (!string.IsNullOrEmpty(entity.PrefabAssetId))
                {
                    MonoBehaviour.Destroy(entity.gameObject);
                }
            }

            // 2. Re-instantiate or populate entities from file
            foreach (var saveData in activeRun.entitySaveDatas)
            {
                SaveEntity targetEntity = null;

                // Check if object already exists in scene (e.g. static managers like VoucherManager)
                if (activeEntities.TryGetValue(saveData.saveKey, out var existing))
                {
                    targetEntity = existing;
                }
                // If it's a dynamic object (e.g. Card/Joker), instantiate via Prefab Database
                else if (!string.IsNullOrEmpty(saveData.prefabAssetId))
                {
                    GameObject prefab = saveSettingData.GetPrefab(saveData.prefabAssetId);
                    if (prefab != null)
                    {
                        GameObject spawned = MonoBehaviour.Instantiate(prefab);
                        targetEntity = spawned.GetComponent<SaveEntity>();
                    }
                }

                // Restore all tagged field state across all scripts on the target
                targetEntity?.RestoreState(saveData.componentsState);
            }
            Debug.Log("Load Complete");
        }
        static void RunGlobalPrepareForLoad()
        {
            foreach (var item in activeEntities)
            {
                item.Value.GlobalPrepareForLoad();
            }
        }
        static async Task<MasterSaveData> LoadMasterSaveDataAsync()
        {
            return await Task.Run(() =>
            {
                string json = GetSavedData();
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log("Load failed, save data empty");
                    return null;
                }

                try
                {
                    return JsonConvert.DeserializeObject<MasterSaveData>(json, ReflectionSaveEngine._cachedSettings);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveSystem] Failed to load save file: {ex.Message}");
                    return null;
                }
            });
        }
        static string GetSavedData()
        {
            string targetPath = SaveFilePath;

            // Fallback to backup if main save is missing
            if (!File.Exists(targetPath))
            {
                Debug.LogWarning("[SaveSystem] Main save missing. Attempting to restore from backup.");
                if (!File.Exists(BackupSavePath))
                {
                    Debug.Log("[SaveSystem] No save file found. Returning new instance.");
                    return null;
                }
                targetPath = BackupSavePath;
            }

            try
            {
                string json = File.ReadAllText(targetPath);
                return json;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load save file at {targetPath}: {ex.Message}");

                // Try loading from backup if primary save corrupted
                if (targetPath != BackupSavePath && File.Exists(BackupSavePath))
                {
                    Debug.LogWarning("[SaveSystem] Attempting backup fallback due to primary corruption.");
                    return File.ReadAllText(BackupSavePath);
                }
                return null;
            }
        }
        public static bool HasSaveFile()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("[SaveSystem] Main save missing.");
                if (!File.Exists(BackupSavePath))
                {
                    Debug.Log("[SaveSystem] Backup save missing as well");
                    return false;
                }
            }
            return true;
        }
        public static bool HasActiveRun()
        {
            if (masterSaveData == null)
            {
                Debug.LogWarning("[SaveSystem] Master save data not initialized.");
                return false;
            }
            return masterSaveData.activeRun != null;
        }
    }
}