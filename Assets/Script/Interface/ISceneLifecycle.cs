using System.Threading.Tasks;
using UnityEngine;

public interface ISceneLifecycle
{
    Task OnPrepareUnloadAsync();
    public void ReceiveLoadPayload(object loadPayload);
    Task OnSceneLoadedAsync();
}
