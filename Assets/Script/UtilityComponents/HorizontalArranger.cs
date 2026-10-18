using UnityEngine;

public class HorizontalArranger : MonoBehaviour
{
    public float maxDistanceBetweenCards = 3f;
    public float maxWidth = 10;
    public float zDistanceBetween = .01f;
    private void OnTransformChildrenChanged()
    {
        if (transform.childCount == 0)
            return;
        float distanceBetween = maxDistanceBetweenCards;
        if (maxDistanceBetweenCards * transform.childCount > maxWidth)
        {
            distanceBetween = maxWidth / transform.childCount;
        } 
        float startingPoint = -(distanceBetween / 2) * (transform.childCount -1);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = new Vector3(startingPoint +(i * distanceBetween), 0, i * zDistanceBetween);
        }
    }
}
