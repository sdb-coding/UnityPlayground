using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HierachyColorsContainer", menuName = "EditorData/Hierachy Colors Container")]
public class HierachyColorsContainer : ScriptableObject
{
    [System.Serializable]
    public struct ColorPair
    {
        public Forgotlings.Editor.GameObjectIdentifier gameObject;
        public Color parentColor;
        public Color childColor;
    }
    [System.Serializable]
    public struct SceneSpcific
    {
        public string sceneName;
        public ColorPair[] colorPairs;
    }
    public ColorPair[] generics;
    public List<SceneSpcific> sceneSpecifics;
    public ColorPair[] GetGenerics()
    {
        return generics;
    }
    public ColorPair[] GetSceneColorPairs(string sceneName)
    {
        for (int i = 0; i < sceneSpecifics.Count; i++)
        {
            if (sceneSpecifics[i].sceneName.Equals(sceneName))
            {
                return sceneSpecifics[i].colorPairs;
            }
        }
        return null;
    }
    public ColorPair[] GetColorPairs(string sceneName)
    {
        for (int i = 0; i < sceneSpecifics.Count; i++)
        {
            if (sceneSpecifics[i].sceneName.Equals(sceneName))
            {
                var combined = new ColorPair[generics.Length + sceneSpecifics[i].colorPairs.Length];
                System.Array.Copy(generics, combined, generics.Length);
                System.Array.Copy(sceneSpecifics[i].colorPairs, 0,combined, generics.Length, sceneSpecifics[i].colorPairs.Length);
                return combined;
            }
        }
        return generics;
    }
}
