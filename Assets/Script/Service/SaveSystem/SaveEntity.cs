using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Services.SaveSystem
{
    [DisallowMultipleComponent]
    public class SaveEntity : MonoBehaviour
    {
        [SerializeField] private string customSaveKey;
        [SerializeField] private string prefabAssetId;
        public bool ignoreActiveInHierarchy;
        public string SaveKey => string.IsNullOrEmpty(customSaveKey) ? gameObject.name : customSaveKey;
        public string PrefabAssetId => prefabAssetId;
#if UNITY_EDITOR
        public void SetPrefabAssetID(string prefabAssetId) => this.prefabAssetId = prefabAssetId;
#endif
        private ISaveableReceiver[] receivers;
        private void Awake()
        {
            receivers = GetComponents<ISaveableReceiver>();
        }
        private void Start()
        {
            if (ignoreActiveInHierarchy)
                SaveLoadManager.Register(this);
        }
        private void OnDestroy()
        {
            if (ignoreActiveInHierarchy)
                SaveLoadManager.Unregister(this);
        }
        private void OnEnable()
        {
            if (!ignoreActiveInHierarchy)
                SaveLoadManager.Register(this);
        }
        private void OnDisable()
        {
            if (!ignoreActiveInHierarchy)
                SaveLoadManager.Unregister(this);
        }

        public void GlobalPrepareForLoad()
        {
            for (int i = 0; i < receivers.Length; i++)
            {
                receivers[i].OnBeforeLoadGlobal();
            }
        }
        public Dictionary<string, object> GatherState()
        {
            PrepareForSave();
            var entityState = new Dictionary<string, object>();
            var components = GetComponents<MonoBehaviour>();

            foreach (var comp in components)
            {
                if (comp == this) continue;

                // IF the component hosts a C# Model via IModelProvider:
                if (comp is IModelContainer modelContainer)
                {
                    object model = modelContainer.GetModel();
                    if (model != null)
                    {
                        var modelState = ReflectionSaveEngine.ExtractState(model);
                        if (modelState.Count > 0)
                            entityState[model.GetType().Name] = modelState;
                    }
                }
                // ALSO check for any direct [SaveState] fields on the Presenter itself
                var compState = ReflectionSaveEngine.ExtractState(comp);
                if (compState.Count > 0)
                {
                    entityState[comp.GetType().Name] = compState;
                }
            }

            return entityState;
        }
        public void PrepareForSave()
        {
            for (int i = 0; i < receivers.Length; i++)
            {
                try
                {
                    receivers[i].OnBeforeSaveInstance();
                } catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }

            }
        }


        public void RestoreState(Dictionary<string, object> entityState)
        {
            var components = GetComponents<MonoBehaviour>();
            List<IModelContainer> modelContainers = new List<IModelContainer>();
            foreach (var comp in components)
            {
                if (comp == this) continue;

                // 1. Restore the C# Model state
                if (comp is IModelContainer modelContianer)
                {
                    object model = modelContianer.GetModel();
                    if (model != null && entityState.TryGetValue(model.GetType().Name, out var modelStateObj))
                    {
                        if (modelStateObj is Newtonsoft.Json.Linq.JToken jToken)
                        {
                            // Deserialize directly back onto the target model fields
                            Dictionary<string, object> dictState = jToken.ToObject<Dictionary<string, object>>();
                            ReflectionSaveEngine.RestoreState(model, dictState);
                            modelContainers.Add(modelContianer);
                        }
                        else if (modelStateObj is Dictionary<string, object> modelState)
                        {
                            ReflectionSaveEngine.RestoreState(model, modelState);
                            modelContainers.Add(modelContianer);
                        }
                    }
                }

                // 2. Restore any direct Presenter state
                if (entityState.TryGetValue(comp.GetType().Name, out var compStateObj))
                {
                    if (compStateObj is Dictionary<string, object> compState)
                    {
                        ReflectionSaveEngine.RestoreState(comp, compState);
                    }
                }
            }

            foreach (var item in modelContainers)
            {
                item.ModelRestored();
            }
            CompleteLoad();
        }
        public void CompleteLoad()
        {
            for (int i = 0; i < receivers.Length; i++)
            {
                receivers[i].OnAfterLoadInstance();
            }
        }
        private void Reset()
        {
            if (string.IsNullOrEmpty(customSaveKey))
                customSaveKey = $"{gameObject.name}_{Guid.NewGuid()}";
        }
    }
}