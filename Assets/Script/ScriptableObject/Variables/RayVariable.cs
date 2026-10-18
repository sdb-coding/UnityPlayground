using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName ="Variables", menuName = "Variables/Ray Variable", order = 5)]
public class RayVariable : ScriptableObject
{
    [NonSerialized] public Ray Value;

    public void SetValue(Ray value)
    {
        Value = value;
    }

    public void SetValue(RayVariable value)
    {
        Value = value.Value;
    }
}
