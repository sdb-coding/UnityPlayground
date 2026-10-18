using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Forgotlings.Editor
{
    [System.Serializable]
    public class GameObjectIdentifier
    {
        public string name;
        public string sceneName;
        public Vector3 position;
        public string parentName;
        public int childCount;
        public GameObjectIdentifier UpdateStructFrom(GameObject gameObject)
        {
            name = gameObject.name;
            position = gameObject.transform.transform.position;
            parentName = gameObject.transform.parent == null ? "null" : gameObject.transform.parent.gameObject.name;
            childCount = gameObject.transform.childCount;
            sceneName = gameObject.scene.name;
            return this;
        }
        public GameObjectIdentifier(GameObject gameObject)
        {
            name = gameObject.name;
            position = gameObject.transform.position;
            parentName = gameObject.transform.parent == null ? "null" : gameObject.transform.parent.gameObject.name;
            childCount = gameObject.transform.childCount;
            sceneName = gameObject.scene.name;
        }
        public static GameObject FindGameObject(GameObjectIdentifier objectToFind)
        {
            var namedGameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == objectToFind.name);
            if (namedGameObjects.Count() == 1)
                return namedGameObjects.FirstOrDefault();
            else if (namedGameObjects.Count() == 0)
                return null;

            List<(GameObject, int)> candidates = new List<(GameObject, int)>();
            foreach (var item in namedGameObjects)
            {
                int hits = 0;
                if (item.transform.position == objectToFind.position)
                    hits++;
                if (item.transform.parent != null && item.transform.parent.gameObject.name == objectToFind.parentName)
                    hits++;
                if (item.transform.childCount == objectToFind.childCount)
                    hits++;
                if (item.scene.name == objectToFind.sceneName)
                    hits++;
                //if (item.transform.parent.gameObject.name == objectToFind.parentName)
                //    hits++;

                if (hits != 0)
                    candidates.Add((item, hits));
            }
            if (candidates.Count == 0)
                return namedGameObjects.FirstOrDefault();
            if (candidates.Count == 1)
                return candidates[0].Item1;

            int candidateNumber = -1;
            int highestHits = int.MinValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Item2 > highestHits)
                {
                    candidateNumber = i;
                    highestHits = candidates[i].Item2;
                }
            }
            return candidates[candidateNumber].Item1;
        }
        public static GameObject FindGameObjectInScene(GameObjectIdentifier gameObjectIdentifier, string sceneName)
        {
            var allObjectsWithName = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(o => o.name == gameObjectIdentifier.name).Where(p => p.transform.position == gameObjectIdentifier.position)
                .Where(p => gameObjectIdentifier.parentName == "null" ? p.transform.parent == null : p.transform.parent.name.Equals(gameObjectIdentifier.parentName))
                .Where(u => u.scene != null && u.scene.name != null && u.scene.name.Equals(sceneName));
            if (allObjectsWithName.Count() != 1)
            {
                foreach (var item in allObjectsWithName)
                {
                    Debug.Log($"{item.name} {item.scene.name} {item.transform.position} ");
                }
                Debug.Log($"Something went wrong. Found {allObjectsWithName.Count()} with name: {gameObjectIdentifier.name} when there should be 1");
                return default;
            }
            //Debug.Log(allObjectsWithName.First().name);
            return allObjectsWithName.First();
        }
    }
}
