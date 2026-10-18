using UnityEditor;
using UnityEngine;

public static class ModelDebugShortcut
{
    // [MenuItem] exposes the function to the top menu and binds a hotkey.
    // "_" means no modifiers. "F12" binds it directly to the F12 key.
    [MenuItem("Tools/Debug Active Model _F12")]
    private static void DebugSelectedModel()
    {
        // 1. Ensure the game is actually running
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("[Model Debugger] Game must be running in Play Mode to inspect active Model states.");
            return;
        }

        // 2. Get the currently highlighted GameObject in the Hierarchy/Scene view
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("[Model Debugger] No GameObject selected. Click a GameObject in the Hierarchy first.");
            return;
        }

        // 3. Search the GameObject for any component implementing our interface
        IModelContainer container = selectedObject.GetComponent<IModelContainer>();

        if (container != null)
        {
            object model = container.GetModel();

            // Call the reflection utility we created earlier
            ModelLogger.LogModelState(model, $"{selectedObject.name} Model");
        }
        else
        {
            Debug.LogWarning($"[Model Debugger] '{selectedObject.name}' does not implement IModelContainer.");
        }
    }
}