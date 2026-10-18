using UnityEngine;

public interface IHoverable
{
    public CardType cardType { get; }
    public bool IsHovering { get; }
    public void OnHoverEnter();
    public void OnHoverExit();
    public GameObject gameObject { get; }
    public RectTransform RectTransform{ get; }
    public int Depth { get;}
    public void Hide();
}
