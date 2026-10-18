using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ConvertPointerPositionToWorldSpace : MonoBehaviour
{
    public InputActionReference pointerPosition;
    public RayVariable convertedPointerPosition;
    public Camera mainCamera;
    private void Update()
    {
        convertedPointerPosition.SetValue(mainCamera.GetComponent<Camera>().ScreenPointToRay(pointerPosition.action.ReadValue<Vector2>()));
    }
}
