using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StoreBlueprint", menuName = "Data/Blueprint/StoreBlueprint")]
public class StoreBlueprint : ScriptableObject
{
    public JokerBlueprint[] buyableJokerBlueprints;
    public PlayingCardBlueprint[] buyablePlayingCards;
    public HashSetScriptableObject uniqueObjects;
}
