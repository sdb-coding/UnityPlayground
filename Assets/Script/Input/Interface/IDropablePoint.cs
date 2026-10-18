using UnityEngine;
using System.Collections.Generic;
public interface IDropablePoint
{
    public DropableResponse IsDropAllowed(MonoBehaviour droppedBehaviour);
    //public bool DropIfAllowed(MonoBehaviour droppedBehaviour);
    public void DropObject(MonoBehaviour droppedBehaviour);
    public void RemoveObject(MonoBehaviour droppedBehaviour);
}
