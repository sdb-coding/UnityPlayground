using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IDraggable
{
    public GameObject GameObject { get; }
    public DraggableType DraggableType { get; }
}
public interface IBeginDrag
{
    public void BeginDrag();
}
public interface IEndDrag
{
    public void EndDrag();
}
public interface IOnDrag
{
    public void OnDrag(Vector3 delta);
}

