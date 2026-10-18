using UnityEngine;
using UnityEngine.EventSystems;

public static class EventSystemInitializer
{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;

            GameObject eventSystemGO = new GameObject("Runtime_EventSystem");

            Object.DontDestroyOnLoad(eventSystemGO);

            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            Debug.Log("[EventSystemInitializer] Successfully spawned runtime EventSystem.");
        }
}
