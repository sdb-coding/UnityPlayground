using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Threading;

public class PointerInputDetector : MonoBehaviour
{
    public event Action<Collider2D> onClick;
    public event Action<Collider2D> onDragBegin;
    public event Action onDragRelease;

    //[Header("Scene references")]
    //[SerializeField] PointerRaycaster pointerRaycaster;

    [Header("Input References")]
    public InputActionReference clickAction;
    public InputActionReference positionAction;

    [Header("Settings")]
    public float distanceForDrag = 100;
    public float timeForDrag = .3f;
    [Header("Raycast")]
    public RayVariable convertedPointerPosition;
    public LayerMask detectableLayers;

    System.Threading.CancellationTokenSource detectDragCancellationTokenSource;
    private void Awake()
    {
        PointerRaycaster.Init();
    }
    private void OnEnable()
    {
        clickAction.action.started += SubmitStarted;
        positionAction.action.performed += OnPointerMoved;
    }
    private void OnDisable()
    {
        clickAction.action.started -= SubmitStarted;
        if (detectDragCancellationTokenSource != null &&!detectDragCancellationTokenSource.IsCancellationRequested)
        {
            SubmitReleased(default);
        }
    }
    void CheckForIBeginClickDown()
    {
        if (PointerRaycaster.hitCollider.Value != null)
        {
            var beginClickDowns = PointerRaycaster.hitCollider.Value.GetComponents<IBeginClickDown>();
            foreach (var item in beginClickDowns)
            {
                item.OnBeginClickDown();
            }
        }
    }
    private void OnPointerMoved(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)
    {
        PointerRaycaster.DoRaycast(positionAction.action.ReadValue<Vector2>(), convertedPointerPosition.Value, detectableLayers);
    }
    async void SubmitStarted(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)
    {
        PointerRaycaster.SaveRaycast();
        CheckForIBeginClickDown();
        detectDragCancellationTokenSource = new CancellationTokenSource();
        if (await WaitForMovementOrRelease(detectDragCancellationTokenSource.Token))
        {
            if (onDragBegin!= null)
                onDragBegin(PointerRaycaster.hitCollider.Value);
            clickAction.action.canceled += SubmitReleased;
        }
    }
    void SubmitReleased(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)
    {
        clickAction.action.canceled -= SubmitReleased;
        if (onDragRelease != null)
            onDragRelease();
        if (detectDragCancellationTokenSource != null && !detectDragCancellationTokenSource.IsCancellationRequested)
            detectDragCancellationTokenSource.Cancel();
    }
    async Task<bool> WaitForMovementOrRelease(CancellationToken ct)
    {
        Vector3 startingPosition = positionAction.action.ReadValue<Vector2>();
        var linkedCts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
        //clickAction.action.canceled -= SubmitReleased;
        try
        {
            var checkDelta = CheckIfDeltaIsBigEnough(linkedCts.Token);
            var checkTimer = ChekcIfTimeIsEnough(linkedCts.Token);
            var checkIfButtonReleased = CheckIfUserReleasedButton(linkedCts.Token);
            var finishedTask = await Task.WhenAny(checkDelta, checkTimer, checkIfButtonReleased);

            await finishedTask;
            //linkedCts.Cancel();
            if (clickAction.action.IsPressed()) //if this is still pressed then we are doing a drag. Otherwise if it is listed then we will assume a click;
                return true;
            if (onClick != null)
                onClick(PointerRaycaster.hitCollider.Value);
            return false;
        }
        finally
        {
            linkedCts.Cancel();
        }
    }
    async Task CheckIfDeltaIsBigEnough(System.Threading.CancellationToken ct)
    {
        Vector3 startingPosition = positionAction.action.ReadValue<Vector2>();
        while (!ct.IsCancellationRequested)
        {
            await Task.Yield();
            if (Vector2.Distance(startingPosition, positionAction.action.ReadValue<Vector2>()) > distanceForDrag)
            {
                break;
            }
        }
    }
    async Task ChekcIfTimeIsEnough(System.Threading.CancellationToken ct)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(timeForDrag), ct);
        }
        catch (OperationCanceledException)
        {
        }
    }
    async Task CheckIfUserReleasedButton(System.Threading.CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && clickAction.action.IsPressed())
            {
                await Task.Delay(TimeSpan.FromMilliseconds(30), ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Handled cleanly when token cancels mid-delay
        }
    }
}
