#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
using UnityEngine;

public class KeyVariable : ScriptableObject
{
    public string variableName;
    public string[] variableStateNames = new string[0];
    public bool isPersistent;
#if DEBUG
        public string parent;
        public KeyVariableConnection[] checkConnections;
        public KeyVariableConnection[] setConnections;
        [System.Serializable]
        public class KeyVariableConnection
        {
            public enum ConnectionType {Set, Check }
            public string sceneName;
            public string gameObject;
            public string parent;
        }
#endif
#if UNITY_EDITOR && DEBUG

        public void AddConnection(GameObject gameObject, KeyVariableConnection.ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case KeyVariableConnection.ConnectionType.Set:
                    if (setConnections == null)
                        setConnections = new KeyVariableConnection[0];
                    if (!ContainsConnection(gameObject,setConnections, out int setArrayPosition)
                        && ShouldAddConnection(gameObject))
                    {
                        System.Array.Resize(ref setConnections, setConnections.Length + 1);
                        setConnections[setConnections.Length - 1] = new KeyVariableConnection()
                        {
                            gameObject = gameObject.name,
                            parent = gameObject.transform.parent == null ? "" : gameObject.transform.parent.gameObject.name,
                            sceneName = gameObject.scene.name
                        };
                        System.Reflection.Assembly editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Assembly-CSharp-Editor,")); // ',' included to ignore  Assembly-CSharp-Editor-FirstPass
                        System.Type utilityType = editorAssembly.GetTypes().FirstOrDefault(t => t.FullName.Contains("AssetHelper"));
                        KeysAsset keysAsset = (KeysAsset)utilityType.GetMethod("LoadKeysAsset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(obj: null, parameters: null);
                        EditorUtility.SetDirty(keysAsset);
                        EditorUtility.SetDirty(this);
                    }
                    break;
                case KeyVariableConnection.ConnectionType.Check:
                    if (checkConnections == null)
                        checkConnections = new KeyVariableConnection[0];
                    if (!ContainsConnection(gameObject, checkConnections, out int checkArrayPosition)
                        && ShouldAddConnection(gameObject))
                    {
                        System.Array.Resize(ref checkConnections, checkConnections.Length + 1);
                        checkConnections[checkConnections.Length - 1] = new KeyVariableConnection()
                        {
                            gameObject = gameObject.name,
                            parent = gameObject.transform.parent == null ? "" : gameObject.transform.parent.gameObject.name,
                            sceneName = gameObject.scene.name
                        };
                        System.Reflection.Assembly editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Assembly-CSharp-Editor,")); // ',' included to ignore  Assembly-CSharp-Editor-FirstPass
                        System.Type utilityType = editorAssembly.GetTypes().FirstOrDefault(t => t.FullName.Contains("AssetHelper"));
                        KeysAsset keysAsset = (KeysAsset)utilityType.GetMethod("LoadKeysAsset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(obj: null, parameters: null);
                        EditorUtility.SetDirty(keysAsset);
                        EditorUtility.SetDirty(this);
                    }
                    break;
            }
        }
        public void RemoveConnection(GameObject gameObject, KeyVariableConnection.ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case KeyVariableConnection.ConnectionType.Set:
                    if (setConnections == null)
                        setConnections = new KeyVariableConnection[0];
                    if (ContainsConnection(gameObject, setConnections, out int setArrayPosition))
                    {
                        for (int i = setArrayPosition; i < setConnections.Length-1; i++)
                        {
                            setConnections[i] = setConnections[i + 1];
                        }
                        System.Array.Resize(ref setConnections, setConnections.Length - 1);
                        EditorUtility.SetDirty(this);
                    }
                    break;
                case KeyVariableConnection.ConnectionType.Check:
                    if (checkConnections == null)
                        checkConnections = new KeyVariableConnection[0];
                    if (ContainsConnection(gameObject, checkConnections, out int checkArrayPosition))
                    {
                        for (int i = checkArrayPosition; i < checkConnections.Length - 1; i++)
                        {
                            checkConnections[i] = checkConnections[i + 1];
                        }
                        System.Array.Resize(ref checkConnections, checkConnections.Length - 1);
                        EditorUtility.SetDirty(this);
                    }
                    break;
            }
        }
        bool ContainsConnection(GameObject gameObject, KeyVariableConnection[] array, out int arrayPosition)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].gameObject.Equals(gameObject.name)
                    && array[i].sceneName.Equals(gameObject.scene.name))
                {
                    arrayPosition = i;
                    return true;
                }
            }
            arrayPosition = -1;
            return false;
        }
        bool ShouldAddConnection(GameObject gameObject)
        {
            //if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
            //{
            //    var root = FindRoot(gameObject.transform);
            //    if (root.name == "MainComponents"
            //        || root.name == "Managers"
            //        || root.name == "Fig")
            //        return false;
                    
            //}
            return true;
        }
        Transform FindRoot(Transform transform)
        {
            if (transform.parent == null)
                return transform;
            return FindRoot(transform.parent);
        }
#endif
}
