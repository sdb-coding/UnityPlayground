using UnityEngine;

[CreateAssetMenu(fileName = "SettingsDatabase", menuName = "Data/Databases/SettingsDatabase")]
public class SettingsDatabase : ScriptableObject
{
    [Header("UI")]
    public RefEvent<float> UIHoverForPopuptime { get; private set; }
    [SerializeField] float defaultUIHoverforPopup = 0.3f;
    public RefEvent<float> UIPopupDismissGracePeriod { get; private set; }
    [SerializeField] float defaultUIPopupDismissGracePeriod = 0.15f;

    public void Init()
    {
        UIHoverForPopuptime = new RefEvent<float>(defaultUIHoverforPopup);
        UIPopupDismissGracePeriod = new RefEvent<float>(defaultUIPopupDismissGracePeriod);
    }
    public void ResetToDefaults()
    {
        UIHoverForPopuptime.Set(defaultUIHoverforPopup);
        UIPopupDismissGracePeriod.Set(defaultUIPopupDismissGracePeriod);
    }
}
