using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablePopupPool<T> where T : MonoBehaviour
{
    private readonly string addressableKey;
    private readonly Transform defaultParent;
    private readonly List<T> pool = new List<T>();

    private T loadedPrefab;
    private AsyncOperationHandle<GameObject> loadHandle;

    public AddressablePopupPool(string addressableKey, Transform defaultParent = null)
    {
        this.addressableKey = addressableKey;
        this.defaultParent = defaultParent;
    }

    public async Task InitializeAsync(int initialSize)
    {
        if (loadedPrefab != null) return;

        loadHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
        GameObject loadedObject = await loadHandle.Task;

        if (loadedObject == null)
        {
            Debug.LogError($"[AddressablePopupPool] Failed to load Addressable with key: {addressableKey}");
            return;
        }

        loadedPrefab = loadedObject.GetComponent<T>();

        for (int i = 0; i < initialSize; i++)
        {
            CreateInstance();
        }
    }

    private T CreateInstance()
    {
        T instance = Object.Instantiate(loadedPrefab, defaultParent);
        instance.gameObject.SetActive(false);
        pool.Add(instance);
        return instance;
    }

    public T GetAvailable()
    {
        foreach (var item in pool)
        {
            if (!item.gameObject.activeSelf)
                return item;
        }

        return CreateInstance();
    }

    public void ReleaseMemory()
    {
        foreach (var item in pool)
        {
            if (item != null) Object.Destroy(item.gameObject);
        }
        pool.Clear();

        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
        }
    }
}