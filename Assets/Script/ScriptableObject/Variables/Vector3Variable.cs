using UnityEngine;
[CreateAssetMenu(fileName ="Variables", menuName = "Variables/Vector3 Variable", order = 5)]
public class Vector3Variable : ScriptableObject 
{
    public Vector3 Value;

    public void SetValue(Vector3 value)
    {
        Value = value;
    }

    public void SetValue(Vector3Variable value)
    {
        Value = value.Value;
    }

    public void ApplyChange(Vector3 amount)
    {
        Value += amount;
    }

    public void ApplyChange(Vector3Variable amount)
    {
        Value += amount.Value;
    }
}
