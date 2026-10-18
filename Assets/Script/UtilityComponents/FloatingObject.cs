using UnityEngine;
using Unity.Cinemachine;
public class FloatingObject : MonoBehaviour, IBeginDrag, IEndDrag
{
    public Transform transformToFloat;
    [Header("Floating Settings")]
    [MinMaxRangeSlider(0, 1)]
    public Vector2 floatAmplitudeMinMax = new Vector2(.1f,.3f);
    float floatAmplitude;
    [MinMaxRangeSlider(0, 1)]
    public Vector2 floatFrequencyMinMax = new Vector2(.4f, .8f);
    float floatFrequency;

    [Header("Rotation Settings")]
    [MinMaxRangeSlider(0, 10)]
    public Vector2 rotationAmplitudeMinMax = new Vector2(1f, 3f);
    float rotationAmplitude;
    [MinMaxRangeSlider(0, 5)]
    public Vector2 rotationFrequencyMinMax = new Vector2(.7f, 1f);
    float rotationFrequency;

    private Quaternion startRotation;
    private float timeOffset;

    public IDropablePoint previousDroppedPoint { get; set; }

    void Start()
    {
        startRotation = transformToFloat.localRotation;

        SetupFrequencyAndAmplitude();

        // Generates a random starting point in the sine cycle (0 to 2*Pi)
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    [ContextMenu("Setup Frequence & Amplitude")]
    void SetupFrequencyAndAmplitude()
    {
        floatAmplitude = Random.Range(floatAmplitudeMinMax.x, floatAmplitudeMinMax.y);
        floatFrequency = Random.Range(floatFrequencyMinMax.x, floatFrequencyMinMax.y);
        rotationAmplitude = Random.Range(rotationAmplitudeMinMax.x, rotationAmplitudeMinMax.y);
        rotationFrequency = Random.Range(rotationFrequencyMinMax.x, rotationFrequencyMinMax.y);
    }

    void Update()
    {
        // Add timeOffset to the current time to randomize the starting position
        float customTime = Time.time + timeOffset;

        // Calculate floating offset
        float newY =Mathf.Sin(customTime * floatFrequency) * floatAmplitude;
        transformToFloat.localPosition = new Vector3(transformToFloat.localPosition.x, newY, transformToFloat.localPosition.z);

        // Calculate rotation offset on the Z axis
        float rotZ = Mathf.Sin(customTime * rotationFrequency) * rotationAmplitude;
        transformToFloat.localRotation = startRotation * Quaternion.Euler(0f, 0f, rotZ);
    }
    #region Draggable
    public void BeginDrag()
    {
        enabled = false;
        transformToFloat.localRotation = Quaternion.identity;
        transformToFloat.localPosition = Vector3.zero;
    }
    public void EndDrag()
    {
        enabled = true;
    }
    #endregion //Draggable
}
