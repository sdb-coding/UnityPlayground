using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardDragger : MonoBehaviour
{
    public event System.Action<IDraggable[]> onBeginDrag;
    public event System.Action onEndOfDrag;

    [Header("Scene References")]
    public PointerInputDetector pointerInputDetector;
    public CardSelection cardSelection;
    public CardDraggable cardDraggable;

    [Header("Project References")]
    public RayVariable convertedPointerPosition;
    
    [Header("Settings")]
    public float distanceForDrag = 100;
    public float timeForDrag = .3f;

    [Header("DraggableTypes")]
    public DraggableType cardPresenterDraggableType;
    
    Coroutine onDragCoroutine;
    System.Threading.CancellationTokenSource onDragTaskCancelationToken;
    private void OnEnable()
    {
        pointerInputDetector.onDragBegin += OnDragBegin;
        pointerInputDetector.onDragRelease += OnDragRelease;
    }
    private void OnDisable()
    {
        pointerInputDetector.onDragBegin -= OnDragBegin;
        pointerInputDetector.onDragRelease -= OnDragRelease;
        if (onDragCoroutine != null || (onDragTaskCancelationToken != null && !onDragTaskCancelationToken.IsCancellationRequested))
        {
            OnDragRelease();
        }
    }
    void OnDragBegin(Collider2D draggedCollider)
    {
        if (draggedCollider == null)
            return;
        var pointerDraggables = draggedCollider.GetComponents<IDraggable>();
        //Debug.Log($"Drag Started. Draggables: {pointerRaycaster.numberOfDraggables}");
        if (pointerDraggables.Length == 0)
            return;
        if (cardSelection.HaveSelection && cardSelection.CurrentSelectionContainsCurrentTarget(draggedCollider))
        {
            IDraggable[] selectionDraggables = new IDraggable[cardSelection.selection.Count];
            for (int i = 0; i < cardSelection.selection.Count; i++)
            {
                selectionDraggables[i] = cardSelection.selection[i].GameObject.GetComponent<IDraggable>();
            }
            SetupDrag(selectionDraggables);
            onEndOfDrag += ClearSelectionOnEndOfDrag;
        }
        else
        {
            SetupDrag(pointerDraggables);
        }
    }
    void SetupDrag(IDraggable[] draggables)
    {
        List<IOnDrag> onDrags = new List<IOnDrag>();
        List<IEndDrag> endDrags = new List<IEndDrag>();
        foreach (var draggable in draggables)
        {
            var beginDrags = draggable.GameObject.GetComponentsInChildren<IBeginDrag>();
            foreach (var beginDrag in beginDrags)
            {
                beginDrag.BeginDrag();
            }
            onDrags.AddRange(draggable.GameObject.GetComponentsInChildren<IOnDrag>());
            endDrags.AddRange(draggable.GameObject.GetComponentsInChildren<IEndDrag>());
        }

        cardDraggable.BeginDrag(draggables);
        //onDragCoroutine = StartCoroutine(SendOnDrag(convertedPointerPosition.Value.origin, dragables.ToArray()));
        RunOnDragTask(convertedPointerPosition.Value.origin, onDrags, endDrags);
    }
    void ClearSelectionOnEndOfDrag()
    {
        cardSelection.ClearSelection();
        onEndOfDrag -= ClearSelectionOnEndOfDrag;
    }
    void OnDragRelease()
    {
        if (onDragTaskCancelationToken != null && !onDragTaskCancelationToken.IsCancellationRequested)
            onDragTaskCancelationToken.Cancel();
        if (onEndOfDrag != null)
        {
            onEndOfDrag();
        }
    }
    IEnumerator SendOnDrag(Vector3 lastPosition, IOnDrag[] onDrags)
    {
        Vector3 delta = Vector3.zero;
        yield return null;
        while (true)
        {
            delta = convertedPointerPosition.Value.origin - lastPosition;
            foreach (var draggable in onDrags)
            {
                draggable.OnDrag(delta);
            }
            lastPosition = convertedPointerPosition.Value.origin;
            yield return null;
        }
    }
    async void RunOnDragTask(Vector3 startingPosition, List<IOnDrag> onDrags, List<IEndDrag> endDrag)
    {
        onDragTaskCancelationToken = new System.Threading.CancellationTokenSource();
        await SendOnDragTask(onDragTaskCancelationToken.Token, startingPosition, onDrags, endDrag);
    }
    
    async Task SendOnDragTask(System.Threading.CancellationToken ct, Vector3 lastPosition, List<IOnDrag> onDrags, List<IEndDrag> endDrag)
    {
        Vector3 delta = Vector3.zero;
        await Task.Yield();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                delta = convertedPointerPosition.Value.origin - lastPosition;
                cardDraggable.OnDrag(delta);
                foreach (var item in onDrags)
                {
                    item.OnDrag(delta);
                }
                lastPosition = convertedPointerPosition.Value.origin;
                await Task.Yield();
            }
        }
        finally
        {
            //Debug.Log("Finally");
            cardDraggable.EndDrag();
            foreach (var item in endDrag)
            {
                item.EndDrag();
            }
        }

    }
}
