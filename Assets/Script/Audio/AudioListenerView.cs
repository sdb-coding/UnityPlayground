using System;
using UnityEngine;

public enum AudioListenerState
{
    FollowTarget,
    FollowCameraInZPlane,
    MoveToPosition,
    Detached
}
public class AudioListenerView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AnimationCurve lerpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Func<Vector3> _positionProvider;
    private Func<Quaternion> _rotationProvider;

    private bool isFrozen;
    private bool isLerping;
    private float lerpTimer;
    private float lerpDuration;
    private Vector3 lerpStartPos;
    private Vector3 cachedPosition;
    private Quaternion cachedRotation;
    private void Start()
    {
        if (Services.CameraController.mainCamera != null)
            FollowTarget(Services.CameraController.mainCamera.transform);
    }

    public void Freeze(bool freeze) => isFrozen = freeze;

    public void FollowTarget(Transform target, float lerpTime = -1)
    {
        if (target == null) return;
        SetPositionProvider(() => target.position, lerpTime);
    }

    public void FollowTargetWithLockedZ(Transform target, float lockedZ, float lerpTime = -1)
    {
        if (target == null) return;
        SetPositionProvider(() =>
        {
            Vector3 pos = target.position;
            pos.z = lockedZ;
            return pos;
        }, lerpTime);
    }

    public void MoveToPosition(Vector3 worldPosition, float lerpTime = 1f)
    {
        SetPositionProvider(() => worldPosition, lerpTime);
    }

    public void SetRotationProvider(Func<Quaternion> rotationProvider)
    {
        _rotationProvider = rotationProvider;
    }

    private void SetPositionProvider(Func<Vector3> newProvider, float lerpTime)
    {
        cachedPosition = transform.position;
        _positionProvider = newProvider;

        if (lerpTime > 0)
        {
            isLerping = true;
            lerpTimer = 0f;
            lerpDuration = lerpTime;
            lerpStartPos = transform.position;
        }
        else
        {
            isLerping = false;
        }
    }

    private void Update()
    {
        if (isFrozen || _positionProvider == null)
            return;

        Vector3 targetPosition = _positionProvider();
        Quaternion targetRotation = _rotationProvider != null ? _rotationProvider() : transform.rotation;

        if (isLerping)
        {
            lerpTimer += Time.deltaTime;
            float t = lerpTimer / lerpDuration;
            float evaluatedT = lerpCurve != null ? lerpCurve.Evaluate(t) : t;

            transform.position = Vector3.Lerp(lerpStartPos, targetPosition, evaluatedT);
            transform.rotation = Quaternion.Slerp(cachedRotation, targetRotation, evaluatedT);

            if (lerpTimer >= lerpDuration)
            {
                isLerping = false;
            }
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

        cachedRotation = targetRotation;
    }

    internal void ResetListener()
    {
        isFrozen = false;
        lerpTimer = 0;
        isLerping = false;
    }
}
