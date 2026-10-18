using UnityEngine;
using System.Collections.Generic;

public class CardDestructionFactory : MonoBehaviour
{
    public static CardDestructionFactory Instance;
    Stack<ParticleSystem> unusedParticles = new Stack<ParticleSystem>();
    public ParticleSystem prefab;
    public Transform container;
    private void Start()
    {
        if ( Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public ParticleSystem GetDestructionParticleSystem()
    {
        return ConstructCardDestruction();
    }
    ParticleSystem ConstructCardDestruction()
    {
        if (unusedParticles.TryPop(out ParticleSystem particleSystem))
            return particleSystem;

        var newParticleSystem = Instantiate(prefab, container);
        return newParticleSystem;
    }

    public void DeleteParticleSystem(ParticleSystem particleSystem)
    {
        unusedParticles.Push(particleSystem);
        particleSystem.transform.SetParent(container);
        particleSystem.transform.localPosition = Vector3.zero;
    }
}
