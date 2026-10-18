using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.SceneSystem
{
    public static class SceneLoadingManager
    {
        public static bool IsLoading { get; private set; }
        static object loadPayload;
        static CancellationTokenSource loadingCts;
        public static bool PrepLoadPayload(object data)
        {
            if (IsLoading)
            {
                Debug.LogError($"Payload attempted to be set, while loading is in progress");
                return false;
            }
            if (Services.SaveSystem.SaveLoadManager.SaveInProgress || Services.SaveSystem.SaveLoadManager.LoadInProgress)
            {
                Debug.LogError($"Payload attempted to be set, while saving or load is in progress");
                return false;
            }
            loadPayload = data;
            return true;
        }
        public static bool IsSceneLoaded(string sceneID)
        {
            var scenePath = SceneDatabaseBootstrap.GetScenePath(sceneID);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (scenePath.Equals(SceneManager.GetSceneAt(i)))
                    return true;
            }
            return false;
        }
        public static bool LoadSceneAsync(string sceneID, LoadSceneMode loadSceneMode, Action loadCompletedCallback = null)
        {
            if (IsLoading)
            {
                Debug.LogError($"LoadScene attempted while load already active");
                return false;
            }
            if (Services.SaveSystem.SaveLoadManager.SaveInProgress)
            {
                Debug.LogError($"LoadScene attempted while saving is in progress");
                return false;
            }

            var scenePath = SceneDatabaseBootstrap.GetScenePath(sceneID);
            if (scenePath == null)
            {
                Debug.LogError($"Scene path from id: {sceneID} is not found");
                return false;
            }

            IsLoading = true;
            loadingCts = new CancellationTokenSource();
            _ = LoadSceneAsync(scenePath, loadSceneMode, loadingCts.Token, loadCompletedCallback);

            return true;
        }

        private static async Task LoadSceneAsync(string scenePath, LoadSceneMode loadSceneMode, CancellationToken token, Action loadCompletedCallback)
        {
            Scene newScene = default;
            bool loadSuccessful = false;
            try
            {
                var currentScene = SceneManager.GetActiveScene();

                Task animationTask = NotifyPrepareUnloadAsync(currentScene);
                Task assetLoadTask = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive).AsTask(token);

                // Await both simultaneously. The code proceeds only when BOTH are 100% complete.
                await Task.WhenAll(animationTask, assetLoadTask);
                token.ThrowIfCancellationRequested();

                // 2. Set active scene
                newScene = SceneManager.GetSceneByPath(scenePath);
                if (!newScene.IsValid() || !newScene.isLoaded)
                {
                    throw new InvalidOperationException($"Scene '{scenePath}' was loaded additively but could not be validated or found in the SceneManager layout.");
                }
                SceneManager.SetActiveScene(newScene);
                loadSuccessful = true;

                var iSceneLifecycleInNewScene = NotifyPayloadReceivers(newScene);

                switch (loadSceneMode)
                {
                    case LoadSceneMode.Single:
                        var unloadOp = SceneManager.UnloadSceneAsync(currentScene);
                        await unloadOp.AsTask(token);
                        break;
                    case LoadSceneMode.Additive:
                        break;
                    default:
                        break;
                }

                await NotifySceneLoadedAsync(iSceneLifecycleInNewScene);
                if (loadCompletedCallback != null)
                    loadCompletedCallback();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Scene load for '{scenePath}' was canceled safely.");
                // ROLLBACK: If cancellation happened AFTER loading the new scene but BEFORE unloading the old one, clean it up!
                if (newScene.IsValid() && newScene.isLoaded && !loadSuccessful)
                {
                    _ = SceneManager.UnloadSceneAsync(newScene);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Scene loading critical failure: {e.Message}");
                Debug.LogException(e);
            }
            finally
            {
                CleanupAndUnlock();
            }
        }
        private static async Task NotifyPrepareUnloadAsync(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;

            var rootObjects = scene.GetRootGameObjects();
            var tasks = new List<Task>();

            foreach (var root in rootObjects)
            {
                var components = root.GetComponentsInChildren<ISceneLifecycle>(includeInactive: true);
                foreach (var comp in components)
                {
                    if (comp != null)
                    {
                        // Call the method and store the returned Task
                        tasks.Add(comp.OnPrepareUnloadAsync());
                    }
                }
            }

            // Wait completely until EVERY single transition component finishes its animation
            await Task.WhenAll(tasks);
        }
        private static List<ISceneLifecycle> NotifyPayloadReceivers(Scene scene)
        {
            // Safety check to ensure the scene is valid and ready
            if (!scene.IsValid() || !scene.isLoaded) return null;

            // 1. Grab only the top-level root GameObjects in this specific scene
            GameObject[] rootObjects = scene.GetRootGameObjects();
            List<ISceneLifecycle> componentsToNotify = new List<ISceneLifecycle>();

            // 2. Search down through the hierarchies of those root objects
            foreach (var root in rootObjects)
            {
                // Includes inactive components if you want them to initialize early
                var found = root.GetComponentsInChildren<ISceneLifecycle>(includeInactive: true);
                componentsToNotify.AddRange(found);
            }

            // 3. Execute your custom interface function on every found component
            foreach (var component in componentsToNotify)
            {
                if (component != null)
                {
                    component.ReceiveLoadPayload(loadPayload);
                }
            }
            return componentsToNotify;
        }
        private static async Task NotifySceneLoadedAsync(List<ISceneLifecycle> componentsToNotify)
        {
            var tasks = new List<Task>();

            foreach (var comp in componentsToNotify)
            {
                if (comp != null) tasks.Add(comp.OnSceneLoadedAsync());
            }
            await Task.WhenAll(tasks);
        }
        static void CleanupAndUnlock()
        {
            if (loadingCts != null)
                loadingCts.Dispose();
            IsLoading = false;
        }
        public static void CancelLoading()
        {
            if (IsLoading && loadingCts != null)
            {
                loadingCts.Cancel();
            }
        }

        // Updated extension method to respect C# CancellationTokens
        private static Task AsTask(this AsyncOperation asyncOp, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>();

            // Register a callback to cancel the task if the token triggers mid-operation
            var registration = token.Register(() => tcs.TrySetCanceled(token));

            if (asyncOp.isDone)
            {
                registration.Dispose();
                tcs.TrySetResult(true);
            }
            else
            {
                asyncOp.completed += _ =>
                {
                    registration.Dispose();
                    tcs.TrySetResult(true);
                };
            }

            return tcs.Task;
        }

//#if UNITY_EDITOR
//        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//        private static void InitializeCurrentSceneOnBoot()
//        {
//            // 1. If we are already mid-load (e.g. from an automated bootstrapper), step out of the way
//            if (IsLoading) return;

//            // 2. Lock the state machine to prevent double-loading during boot setup
//            IsLoading = true;

//            // 3. Kick off the bootstrap animation sequence on a background task thread
//            _ = RunEditorBootSequenceAsync();
//        }

//        private static async Task RunEditorBootSequenceAsync()
//        {
//            try
//            {
//                Scene activeScene = SceneManager.GetActiveScene();
//                Debug.Log($"[SceneLoadingManager] Editor/Boot initialization triggered for scene: '{activeScene.name}'");

//                // PHASE 1: Instantly cache references and build dependencies
//                var sceneLifecycleComponents = NotifyPayloadReceivers(activeScene);

//                // PHASE 2: Wait a frame to let Unity link things like Virtual Cameras/Physics, 
//                // then play all intro/entry animations (like your FadeScreenTransition) simultaneously
//                await Task.Yield();
//                await NotifySceneLoadedAsync(sceneLifecycleComponents);
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogException(e);
//            }
//            finally
//            {
//                // Clean up the global flags and release input systems automatically!
//                CleanupAndUnlock();
//                Debug.Log("[SceneLoadingManager] Boot sequence complete. Inputs freed.");
//            }
//        }
//#endif
    }
}
