using UnityEditor;

public static class AssetHelper
{
    public static string annePortraitSpritesheetPath = "Assets/Graphics/Characters/Anne/anne_thinking_portrait.png";
    public static string editorDataPath = "Assets/Data/EditorData/";
    public static string dataPath = "Assets/Data/";
    //Collections

    public static HierachyColorsContainer LoadHierachyColorsContainer()
    {
        return AssetDatabase.LoadAssetAtPath<HierachyColorsContainer>(editorDataPath + "HierachyColorsContainer.asset");
    }
    public static KeysAsset LoadKeysAsset()
    {
        return AssetDatabase.LoadAssetAtPath<KeysAsset>(dataPath + "Keys/KeysAsset.asset");
    }
}
