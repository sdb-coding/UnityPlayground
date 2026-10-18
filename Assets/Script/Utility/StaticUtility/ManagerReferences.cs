using UnityEngine;

public class ManagerReferences
{
    static Canvas mainCanvas;
    public static Canvas MainCanvas
    {
        get
        {
            if (mainCanvas == null)
                mainCanvas = CreateRootCanvasReference();
            return mainCanvas;
        }
    }
    public static Canvas CreateRootCanvasReference()
    {
        var canvas = MonoBehaviour.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasRoot = new GameObject();
            canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasRoot.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        return canvas;
    }
}