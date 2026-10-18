using UnityEngine;

[System.Serializable]
public class SceneReference : ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector] private string sceneId;

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif

    public string SceneId => sceneId;

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            if (UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating)
                return;
            sceneId = SceneDatabaseEditorUtility.GetOrRegisterScene(sceneAsset);
        }
#endif
    }

    public void OnAfterDeserialize() { }
}