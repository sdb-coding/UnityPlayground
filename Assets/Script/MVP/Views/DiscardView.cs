using UnityEngine;
using System.Collections.Generic;

public class DiscardView : MonoBehaviour
{
    public SpriteRenderer openGraphics;
    public SpriteRenderer closedGraphics;
    public SpriteRenderer hovedGraphics;
    private void OnEnable()
    {
        openGraphics.enabled = false;
        closedGraphics.enabled = true;
        hovedGraphics.enabled = false;
    }
    public void EnableHover()
    {
        hovedGraphics.enabled = true;
    }
    public void DisableHover() 
    {
        hovedGraphics.enabled = false;
    }
    public void Open()
    {
        openGraphics.enabled = true;
        closedGraphics.enabled = false;
    }
    public void Close()
    {
        openGraphics.enabled = false;
        closedGraphics.enabled = true;
    }

}
