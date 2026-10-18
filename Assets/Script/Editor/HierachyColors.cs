using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public static class HierachyColors
{
    class Pair
    {
        //public GameObject gameObject;
        public Color color;
        public Color childColor;
        public string name;
        public EntityId entityID;
        public EntityId[] children;
        public Pair(HierachyColorsContainer.ColorPair colorPair)
        {
            color = colorPair.parentColor;
            childColor = colorPair.childColor;
            name = colorPair.gameObject.name;
        }
    }
    private static HierachyColorsContainer hierachyColorsContainer;
    static List<Pair> ScenePairs;
    static HierachyColors()
    {
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= HierarchyWindowItemOnGui;
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += HierarchyWindowItemOnGui;
        hierachyColorsContainer = AssetHelper.LoadHierachyColorsContainer();
        SceneChange(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), UnityEditor.SceneManagement.OpenSceneMode.Single);
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneChange;
    }
    static void SetupPairs(UnityEngine.SceneManagement.Scene scene)
    {
        //var colorPairs = hierachyColorsContainer.GetColorPairs(scene.name);
        var generics = hierachyColorsContainer.GetGenerics();
        var sceneSpecifics = hierachyColorsContainer.GetSceneColorPairs(scene.name);
        //ScenePairs = new List<Pair>
        //{
        //    new Pair{ color = new Color(1f, 0,0, 0.3f), childColor =new Color(1f, 0,0, 0.1f), name = "Cutscenes"},
        //    new Pair{ color = new Color(0, 1f,0, 0.3f), childColor =new Color(0, 1f,0, 0.1f), name = "OnSceneLoads"},
        //    new Pair{ color = new Color(0, 0,1f, 0.3f), childColor =new Color(0, 0,1f, 0.1f), name = "Dialogues"},
        //};
        ScenePairs = new List<Pair>();

        for (int i = 0; i < generics.Length; i++)
        {
            var namedGameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == generics[i].gameObject.name);
            foreach (var item in namedGameObjects)
            {
                var pair = new Pair(generics[i]);

                pair.entityID = item.GetEntityId();
                pair.children = new EntityId[item.transform.childCount];
                for (int j = 0; j < pair.children.Length; j++)
                {
                    pair.children[j] = item.transform.GetChild(j).gameObject.GetEntityId();
                }
                ScenePairs.Add(pair);
            }
        }
        if (sceneSpecifics == null)
            return;
        for (int i = 0; i < sceneSpecifics.Length; i++)
        {
            var pair = new Pair(sceneSpecifics[i]);
            var gameObject = Forgotlings.Editor.GameObjectIdentifier.FindGameObject(sceneSpecifics[i].gameObject);
            if (gameObject != null)
            {
                pair.entityID = gameObject.GetEntityId();
                pair.children = new EntityId[gameObject.transform.childCount];
                for (int j = 0; j < pair.children.Length; j++)
                {
                    pair.children[j] = gameObject.transform.GetChild(j).gameObject.GetEntityId();
                }
            }
            else
            {
                pair.children = new EntityId[0];
            }
            ScenePairs.Add(pair);
        }
    }
    static void SceneChange(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        if (mode == UnityEditor.SceneManagement.OpenSceneMode.Single)
        {
            SetupPairs(scene);
        }
    }
    private static void HierarchyWindowItemOnGui(EntityId entityId, Rect selectionRect)
    {
        for (int i = 0; i < ScenePairs.Count; i++)
        {
            if (ScenePairs[i].entityID.Equals(entityId))
            {
                EditorGUI.DrawRect(selectionRect, ScenePairs[i].color);
                break;
            }
            var obj = EditorUtility.EntityIdToObject(entityId);
            for (int j = 0; j < ScenePairs[i].children.Length; j++)
            {
                if (ScenePairs[i].children[j].Equals(entityId))
                {
                    EditorGUI.DrawRect(selectionRect, ScenePairs[i].childColor);
                    break;
                }
            }
        }
        return;
        //var obj = EditorUtility.InstanceIDToObject(instanceID) as UnityEngine.Object;
        //if (obj == null)
        //    return;

        //if (obj is GameObject)
        //{
        //    for (int i = 0; i < pairs.Count; i++)
        //    {
        //        if (obj.Equals(pairs[i].gameObject))
        //        {
        //            EditorGUI.DrawRect(selectionRect, pairs[i].color);
        //            break;
        //        }
        //    }

            // Level Area Specific GUI
            //var levelArea = (obj as GameObject).GetComponent<LevelArea>();
            //if (levelArea != null)
            //{
            //    EditorGUI.DrawRect(selectionRect, levelAreaColors != null ? levelAreaColors.LevelHierarchyOverlay : new Color(0.7f, 0.7f, 0f, 0.1f));

            //    if (levelArea.isHiddenInHieararchy)
            //    {
            //        // showing icon for unlocking a hidden level area
            //        if (GUI.Button(new Rect(selectionRect.xMax - _iconSize * 3, selectionRect.center.y - (_iconSize / 2f), _iconSize, _iconSize),
            //            EditorGUIUtility.IconContent("LevelAreaManager/HiddenIcon.png"), GUIStyle.none))
            //        {
            //            LevelAreaManagerEditor.SetAreaHiddenState(levelArea, true);
            //        }
            //    }
            //}

            //if (obj.name == "Bridges")
            //{
            //    EditorGUI.DrawRect(selectionRect, levelAreaColors != null ? levelAreaColors.BridgeHierarchyOverlay : new Color(0.7f, 0.7f, 0f, 0.1f));
            //}
        //}
    }
    
}
