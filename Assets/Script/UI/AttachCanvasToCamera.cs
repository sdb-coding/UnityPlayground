using UnityEngine;
using System.Collections.Generic;
using Services;

public class AttachCanvasToCamera : MonoBehaviour
{
    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        if (CameraController.mainCamera != null && canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = CameraController.mainCamera;
        }
        Destroy(this);
    }
}
