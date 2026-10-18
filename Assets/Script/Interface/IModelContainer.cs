using UnityEngine;
using System.Collections.Generic;

public interface IModelContainer
{
    public object GetModel();
    public void ModelRestored();
}
