using UnityEngine;
using System.Collections.Generic;

public class DeckView : MonoBehaviour
{
    public SpriteRenderer backSide;
    
    public void UpdateSuit(Sprite newDeckGraphics)
    {
        backSide.sprite = newDeckGraphics;
    }
}
