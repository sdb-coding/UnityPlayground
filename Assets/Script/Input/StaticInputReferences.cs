using UnityEngine;
using UnityEngine.InputSystem;

public static class StaticInputReferences
{
    const string pointActionString = "UI/Point";
    static InputAction pointAction;
    public static InputAction PointAction
    {
        get
        {
            if (pointAction == null)
                pointAction = InputSystem.actions.FindAction(pointActionString);
            return pointAction;
        }
    }
}
