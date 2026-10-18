using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class LocalDevCameraRig : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera virtualCamera;

    private void Awake()
    {
        Camera devCamera = GetComponent<Camera>();

        virtualCamera.ForceCameraPosition(transform.position, transform.rotation);

        if (devCamera != null)
        {
            virtualCamera.Lens.FieldOfView = devCamera.fieldOfView;
            virtualCamera.Lens.NearClipPlane = devCamera.nearClipPlane;
            virtualCamera.Lens.FarClipPlane = devCamera.farClipPlane;
            if (devCamera.orthographic)
            {
                virtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
                virtualCamera.Lens.OrthographicSize = devCamera.orthographicSize;
            }
            else
            {
                virtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            }
        }

        var urpCameraData = GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (urpCameraData != null)
        {
            urpCameraData.enabled = false;
        }

        if (devCamera != null)
        {
            devCamera.enabled = false;
        }

        Destroy(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponentInChildren<CinemachineCamera>();
            if (virtualCamera == null)
                virtualCamera = gameObject.AddComponent<CinemachineCamera>();
        }
    }
#endif
}