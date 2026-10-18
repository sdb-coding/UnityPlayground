using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
[CustomEditor(typeof(KeysAsset))]
public class KeysAsset_Editor : Editor
{
#if DEBUG
        //private ReorderableList persistentReorderableList;
        //private ReorderableList reorderableList;
        private KeysAsset myTarget;
        //private List<AnimBool> toggleTemplatesUI = new List<AnimBool>();

        private List<SerializedObject> serializedKeyVariableObjects = new List<SerializedObject>();
        private List<SerializedObject> serializedPersistentKeyVariableObjects = new List<SerializedObject>();

        void OnEnable()
        {
            myTarget = target as KeysAsset;
            SetupSortedView(serializedObject.FindProperty(nameof(KeysAsset.persistentVariables)),ref persistentSorted, false);
            SetupSortedView(serializedObject.FindProperty(nameof(KeysAsset.globalVariables)), ref globalSorted, true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Collapse All", GUILayout.MaxWidth(80)))
                {
                    for (int i = 0; i < persistentSorted.parents.Length; i++)
                    {
                        persistentSorted.parents[i].parent.unfolded = false;
                    }
                    for (int i = 0; i < globalSorted.parents.Length; i++)
                    {
                        globalSorted.parents[i].parent.unfolded = false;
                    }
                    myTarget.showUnparentedGlobal = false;
                    myTarget.showUnparentedPersistent = false;
                }
                EditorGUILayout.EndHorizontal();
                SortedView();

            serializedObject.ApplyModifiedProperties();
        }
        #region Sorted view
        public class KeyVariableOrderedObject
        {
            public int position;
            public bool unfolded;
        }
        public struct KeyVariableParent
        {
            public KeyVariableParent(KeysAsset.Parent parent, int index)
            {
                this.index = index;
                this.parent = parent;
                list = new List<KeyVariableOrderedObject>();
            }
            public int index;
            public KeysAsset.Parent parent;
            public List<KeyVariableOrderedObject> list;
        }
        public class KeyVariableType
        {
            //public enum KeyVariableTypeGroup { Global, Persistent}
            //public KeyVariableTypeGroup thisGroup;
            public KeyVariableOrderedObject[] keyVariablesOrdered;
            public KeyVariableParent[] parents;
            public string[] parentOptions;
            public List<KeyVariableOrderedObject> unparented;

        }
        KeyVariableType persistentSorted;
        KeyVariableType globalSorted;

        //KeyVariableOrderedObject[] keyVariablesOrdered;
        //KeyVariableParent[] parents;
        //string[] parentOptions;
        //List<KeyVariableOrderedObject> unparented;
        Color firstColor;
        Color secondColor;
        Color baseColor;
        void SetupSortedView(SerializedProperty property,ref KeyVariableType keyVariableType, bool isGlobal)
        {
            keyVariableType = new KeyVariableType();
            keyVariableType.keyVariablesOrdered = new KeyVariableOrderedObject[isGlobal ? myTarget.globalVariables.Count : myTarget.persistentVariables.Count];
            // Setup parents
            keyVariableType.parents = new KeyVariableParent[isGlobal ? myTarget.globalParents.Count : myTarget.persistentParents.Count];
            keyVariableType.parentOptions = new string[(isGlobal ? myTarget.globalParents.Count : myTarget.persistentParents.Count) + 1 ];
            keyVariableType.parentOptions[keyVariableType.parentOptions.Length - 1] = "None";
            keyVariableType.unparented = new List<KeyVariableOrderedObject>();
            for (int i = 0; i < keyVariableType.parents.Length; i++)
            {
                keyVariableType.parents[i] = new KeyVariableParent(isGlobal ? myTarget.globalParents[i] : myTarget.persistentParents[i], i);
                keyVariableType.parentOptions[i] = isGlobal ? myTarget.globalParents[i].parentName : myTarget.persistentParents[i].parentName;
            }

            var sortedList = new List<KeyVariable>();
            int variableCount = isGlobal ? myTarget.globalVariables.Count : myTarget.persistentVariables.Count;
            var arrayToCopyFrom = isGlobal ? myTarget.globalVariables : myTarget.persistentVariables;
            for (int i = 0; i < variableCount; i++)
            {
                sortedList.Add(arrayToCopyFrom[i]);
            }
            sortedList = sortedList.OrderBy(o => o.variableName).ToList();
            for (int i = 0; i < sortedList.Count; i++)
            {
                keyVariableType.keyVariablesOrdered[i] = new KeyVariableOrderedObject() { position = arrayToCopyFrom.IndexOf(sortedList[i]), unfolded = false };
                if (string.IsNullOrEmpty(sortedList[i].parent))
                {
                    keyVariableType.unparented.Add(keyVariableType.keyVariablesOrdered[i]);
                }
                else
                {
                    bool parentFound = false;
                    for (int j = 0; j < keyVariableType.parents.Length; j++)
                    {
                        if (keyVariableType.parents[j].parent.parentName.Equals(sortedList[i].parent))
                        {
                            keyVariableType.parents[j].list.Add(keyVariableType.keyVariablesOrdered[i]);
                            parentFound = true;
                            break;
                        }
                    }
                    if (!parentFound)
                    {
                        Debug.Log("nope this doesn't happen");
                    }
                }
            }
            
            searchText = "";
            baseColor = GUI.backgroundColor;
            firstColor = baseColor;
            secondColor = baseColor;
            firstColor.r += 0.1f;
            firstColor.g += .1f;
            firstColor.b += .1f;

            secondColor.r -= 0.1f;
            secondColor.g -= .1f;
            secondColor.b -= .1f;
        }
        void SortedView()
        {
            DrawSearchGUI();
            
            if (refresh)
            {
                SetupSortedView(serializedObject.FindProperty(nameof(KeysAsset.persistentVariables)), ref persistentSorted, false);
                SetupSortedView(serializedObject.FindProperty(nameof(KeysAsset.globalVariables)), ref globalSorted, true);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                EditorUtility.SetDirty(target);
                refresh = false;
            }
            //var propertyDrawer = PropertyDrawerUtility.FindDrawerForType(typeof(KeyVariable));
            if (searching)
            {
                GUILayout.Label("Persistent");
                DisplaySearchedView(serializedObject.FindProperty(nameof(KeysAsset.persistentVariables)), persistentSorted, false);
                GUILayout.Space(10);
                GUILayout.Label("Global");
                if (!refresh)
                    DisplaySearchedView(serializedObject.FindProperty(nameof(KeysAsset.globalVariables)), globalSorted, true);
            }
            else
            {
                GUILayout.Label("Persistent");
                DisplayWithParents(serializedObject.FindProperty(nameof(KeysAsset.persistentVariables)), persistentSorted, false);
                GUILayout.Space(10);
                GUILayout.Label("Global");
                if (!refresh)
                    DisplayWithParents(serializedObject.FindProperty(nameof(KeysAsset.globalVariables)), globalSorted, true);

            }
            
            GUI.color = baseColor;
        }
        void DisplaySearchedView(SerializedProperty variables, KeyVariableType keyVariableType, bool isGlobal)
        {
            for (int i = 0; i < variables.arraySize; i++)
            {
                var keyVariable = new SerializedObject(variables.GetArrayElementAtIndex(keyVariableType.keyVariablesOrdered[i].position).objectReferenceValue);
                var variableName = keyVariable.FindProperty(nameof(KeyVariable.variableName));
                var stateNames = keyVariable.FindProperty(nameof(KeyVariable.variableStateNames));
                if (!IsPartOfSearch(variableName, stateNames))
                    continue;
                GUI.color = i % 2 == 0 ? firstColor : secondColor;
                DisplayVariable(variables, keyVariable, variableName, stateNames, keyVariableType.keyVariablesOrdered[i], keyVariableType, isGlobal,- 1);
                //gUIContent.text = serialiProperty.stringValue;

                //propertyDrawer.OnGUI(new Rect(0,50 * i,50,50), keyVariable,GUIContent.none);
                //EditorGUILayout.PropertyField();
            }
        }
        void DisplayWithParents(SerializedProperty variables, KeyVariableType keyVariableType, bool isGlobal)
        {            
            var parentsProperty = serializedObject.FindProperty(isGlobal ? nameof(KeysAsset.globalParents) : nameof(KeysAsset.persistentParents));
            for (int i = 0; i < keyVariableType.parents.Length; i++)
            {
                var unfolded = parentsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(KeysAsset.Parent.unfolded));
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(unfolded.boolValue ? "v": ">", GUILayout.Width(30)))
                {
                    unfolded.boolValue = !unfolded.boolValue;
                }
                EditorGUI.BeginChangeCheck();
                var parentNameProperty = parentsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(KeysAsset.Parent.parentName));
                var preName = parentNameProperty.stringValue;
                EditorGUILayout.PropertyField(parentNameProperty, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    if (CanRename(parentNameProperty.stringValue, keyVariableType.parents[i], keyVariableType))
                    {
                        RenameParent(variables, parentNameProperty.stringValue, keyVariableType.parents[i]);
                    }
                    else
                    {
                        parentNameProperty.stringValue = preName;
                    }
                    refresh = true;
                }
                keyVariableType.parents[i].parent.important = EditorGUILayout.Toggle(keyVariableType.parents[i].parent.important, GUILayout.Width(30));
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    RenameParent(variables, "", keyVariableType.parents[i]);
                    parentsProperty.DeleteArrayElementAtIndex(i);
                    refresh = true;
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (unfolded.boolValue)
                {
                    for (int j = 0; j < keyVariableType.parents[i].list.Count; j++)
                    {

                        var keyVariable = new SerializedObject(variables.GetArrayElementAtIndex(keyVariableType.parents[i].list[j].position).objectReferenceValue);
                        var variableName = keyVariable.FindProperty(nameof(KeyVariable.variableName));
                        var stateNames = keyVariable.FindProperty(nameof(KeyVariable.variableStateNames));
                        switch (DisplayVariable(variables, keyVariable, variableName, stateNames, keyVariableType.parents[i].list[j], keyVariableType, isGlobal, i))
                        {
                            case RefreshOutcome.No:
                                break;
                            case RefreshOutcome.Yes:
                                refresh = true;
                                break;
                            case RefreshOutcome.Immediately:
                                refresh = true;
                                return;
                            case RefreshOutcome.Deleted:
                                keyVariableType.parents[i].list.RemoveAt(j);
                                j--;
                                refresh = true;
                                EditorUtility.SetDirty(myTarget);
                                serializedObject.Update();
                                return;
                        }
                    }
                }
                EditorGUILayout.Space(15, false);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button((isGlobal ? myTarget.showUnparentedGlobal : myTarget.showUnparentedPersistent) ? "v" : ">", GUILayout.Width(30)))
            {
                if (isGlobal)
                    myTarget.showUnparentedGlobal = !myTarget.showUnparentedGlobal;
                else
                    myTarget.showUnparentedPersistent = !myTarget.showUnparentedPersistent;
            }
            EditorGUILayout.LabelField("UnParented");
            EditorGUILayout.EndHorizontal();
            if (!refresh && isGlobal ? myTarget.showUnparentedGlobal : myTarget.showUnparentedPersistent)
            {
                for (int i = 0; i < keyVariableType.unparented.Count; i++)
                {
                    var keyVariable = new SerializedObject(variables.GetArrayElementAtIndex(keyVariableType.unparented[i].position).objectReferenceValue);
                    var variableName = keyVariable.FindProperty(nameof(KeyVariable.variableName));
                    var stateNames = keyVariable.FindProperty(nameof(KeyVariable.variableStateNames));
                    switch (DisplayVariable(variables, keyVariable, variableName, stateNames, keyVariableType.unparented[i], keyVariableType, isGlobal, keyVariableType.parentOptions.Length - 1))
                    {
                        case RefreshOutcome.No:
                            break;
                        case RefreshOutcome.Yes:
                            refresh = true; 
                            break;
                        case RefreshOutcome.Immediately:
                        case RefreshOutcome.Deleted:
                            refresh = true;
                            return;
                    }
                }
            }

            if (GUILayout.Button("Add Parent"))
            {
                parentsProperty.arraySize++;
                refresh = true;
            }
            EditorGUILayout.BeginHorizontal();
            newVariableName = EditorGUILayout.TextField(newVariableName);
            if (GUILayout.Button("Add Variable", GUILayout.Width(100)))
            {
                if (string.IsNullOrEmpty(newVariableName))
                {
                    Debug.LogError("Please write a variable name before creating one");
                } else if (VariableNameExist(newVariableName,variables))
                {
                    Debug.LogError("Variable name already exist");
                }
                else
                {
                    refresh = true;
                    newVariable = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            //if (refresh)
            //    SetupSortedView();
            if (newVariable)
            {
                var key = ScriptableObject.CreateInstance<KeyVariable>();
                key.variableName = newVariableName;
                key.name = "Key_" + key.variableName;
                key.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(key, serializedObject.targetObject);

                if(isGlobal)
                    myTarget.globalVariables.Add(key);
                else
                    myTarget.persistentVariables.Add(key);
                EditorUtility.SetDirty(serializedObject.targetObject);
                AssetDatabase.SaveAssets();
                newVariable = false;
            }
        }
        bool refresh = false;
        bool newVariable;
        string newVariableName = "";
        private bool CanRename(string newName, KeyVariableParent keyVariableParent, KeyVariableType keyVariableType)
        {
            for (int j = 0; j < keyVariableType.parents.Length; j++)
            {
                if (keyVariableType.parents[j].Equals(keyVariableParent))
                    continue;
                if (newName.Equals(keyVariableType.parents[j].parent.parentName))
                {
                    Debug.LogError($"Can't have the same parent name!");
                    return false;
                }
            }
            return true;
        }
        private void RenameParent(SerializedProperty variables, string newName, KeyVariableParent keyVariableParent)
        {
            bool sameName = false;
            
            if (!sameName)
            {
                for (int j = 0; j < keyVariableParent.list.Count; j++)
                {
                    var keyVariable = new SerializedObject(variables.GetArrayElementAtIndex(keyVariableParent.list[j].position).objectReferenceValue);

                    keyVariable.FindProperty(nameof(KeyVariable.parent)).stringValue = newName;
                    keyVariable.ApplyModifiedProperties();
                }
            }
        }
        bool VariableNameExist(string newName, SerializedProperty variables)
        {
            for (int i = 0; i < variables.arraySize; i++)
            {
                var keyVariable = new SerializedObject(variables.GetArrayElementAtIndex(i).objectReferenceValue);
                var stringInPosition = keyVariable.FindProperty(nameof(KeyVariable.variableName));
                if (stringInPosition.stringValue.Equals(newName))
                    return true;
            }
            return false;
        }
        public enum RefreshOutcome {No, Yes, Immediately, Deleted }
        RefreshOutcome DisplayVariable(SerializedProperty variables, SerializedObject keyVariable, SerializedProperty variableName, SerializedProperty stateNames, KeyVariableOrderedObject index, KeyVariableType keyVariableType, bool isGlobal,int parentIndex = -1)
        {
            EditorGUILayout.BeginHorizontal();
            
            Color baseContentColor = GUI.contentColor;
            EditorGUILayout.PropertyField(variableName, GUIContent.none);
            GUI.contentColor = stateNames.arraySize == 0 ? Color.cyan : baseContentColor;
            if (GUILayout.Button(stateNames.arraySize.ToString(), GUILayout.Width(30)))
            {
                index.unfolded = !index.unfolded;
            }
            GUI.contentColor = baseContentColor;
            if (parentIndex == -1)
            {
                var parentName = keyVariable.FindProperty(nameof(KeyVariable.parent));
                EditorGUILayout.LabelField(string.IsNullOrEmpty(parentName.stringValue) ? "": parentName.stringValue, GUILayout.Width(80));
            }
            else
            {
                int newIndex = EditorGUILayout.Popup(parentIndex, keyVariableType.parentOptions, GUILayout.Width(50));
                if (newIndex != parentIndex)
                {
                    if (newIndex == keyVariableType.parents.Length)
                    {
                        if (!isGlobal)
                            myTarget.persistentVariables[index.position].parent = "";
                        else
                            myTarget.globalVariables[index.position].parent = "";
                    }
                    else
                    {
                        if (isGlobal)
                            myTarget.globalVariables[index.position].parent = keyVariableType.parents[newIndex].parent.parentName;
                        else
                            myTarget.persistentVariables[index.position].parent = keyVariableType.parents[newIndex].parent.parentName;
                    }
                    EditorUtility.SetDirty(myTarget);
                    if (isGlobal)
                    {
                        EditorUtility.SetDirty(myTarget.globalVariables[index.position]);
                        AssetDatabase.SaveAssetIfDirty(myTarget.globalVariables[index.position]);
                    } else
                    {
                        EditorUtility.SetDirty(myTarget.persistentVariables[index.position]);
                        AssetDatabase.SaveAssetIfDirty(myTarget.persistentVariables[index.position]);
                    }

                    AssetDatabase.SaveAssetIfDirty(myTarget);
                    return RefreshOutcome.Yes;
                }
                if (GUILayout.Button(isGlobal ? "^" : "v", GUILayout.Width(30)))
                {
                    if (isGlobal)
                    {
                        myTarget.globalVariables[index.position].parent = "";
                        myTarget.globalVariables.Remove((KeyVariable)keyVariable.targetObject);
                        myTarget.persistentVariables.Add((KeyVariable)keyVariable.targetObject);
                        return RefreshOutcome.Immediately;
                    }
                    else
                    {
                        myTarget.persistentVariables[index.position].parent = "";
                        myTarget.persistentVariables.Remove((KeyVariable)keyVariable.targetObject);
                        myTarget.globalVariables.Add((KeyVariable)keyVariable.targetObject);
                        
                        return RefreshOutcome.Immediately;
                    }
                }

                
            }
            var checkConnections = ((KeyVariable)keyVariable.targetObject).checkConnections;
            int inScene = InScene(checkConnections, EditorSceneManager.GetActiveScene().name);
            if (checkConnections == null)
            {
                GUILayout.Box("0", GUILayout.Width(38));
            }
            else if (GUILayout.Button($"{inScene}({checkConnections.Length})", GUILayout.Width(38)))
            {
                if (inScene != 0)
                {
                    var activeScene = SceneManager.GetActiveScene();
                    var gameObjectArray = new List<GameObject>();

                    for (int i = 0; i < checkConnections.Length; i++)
                    {
                        if (checkConnections[i].sceneName != activeScene.name)
                        {
                            Debug.Log($"{checkConnections[i].sceneName} - {checkConnections[i].gameObject}");
                            continue;
                        }
                        bool foundit = false;
                        foreach (var item in GetAllRootGameObjects())
                        {
                            if (item.name.Equals(checkConnections[i].gameObject) 
                                && ((string.IsNullOrEmpty(checkConnections[i].parent) && item.transform.parent == null)
                                || (item.transform.parent != null && item.transform.parent.name.Equals(checkConnections[i].parent))))
                            {
                                Debug.Log($"{checkConnections[i].sceneName} - {checkConnections[i].gameObject}", item.gameObject);
                                foundit = true;
                                gameObjectArray.Add(item.gameObject);
                                break;
                            }
                        }
                        if (!foundit)
                            Debug.Log($"Not Found, but should be here: {checkConnections[i].sceneName} - {checkConnections[i].gameObject} Parent: {checkConnections[i].parent}");
                    }
                    if (gameObjectArray.Count != 0)
                        Selection.objects = gameObjectArray.ToArray();
                }
                else
                {
                    for (int i = 0; i < checkConnections.Length; i++)
                    {
                        Debug.Log($"{checkConnections[i].sceneName} - {checkConnections[i].gameObject}");
                    }
                }
            }
            var setConnections = ((KeyVariable)keyVariable.targetObject).setConnections;
            inScene = InScene(setConnections, EditorSceneManager.GetActiveScene().name);

            if (setConnections == null)
            {
                GUILayout.Box("0", GUILayout.Width(38));
            }
            else if (GUILayout.Button($"{inScene}({setConnections.Length})", GUILayout.Width(38)))
            {
                if (inScene != 0)
                {
                    var activeScene = SceneManager.GetActiveScene();
                    var gameObjectArray = new List<GameObject>();
                    for (int i = 0; i < setConnections.Length; i++)
                    {
                        if (setConnections[i].sceneName != activeScene.name)
                        {
                            Debug.Log($"{setConnections[i].sceneName} - {setConnections[i].gameObject}");
                            continue;
                        }
                        bool foundIt = false;
                        foreach (var item in GetAllRootGameObjects())
                        {
                            if (item.name.Equals(setConnections[i].gameObject)
                                && ((string.IsNullOrEmpty(setConnections[i].parent) && item.transform.parent == null)
                                || (item.transform.parent != null && item.transform.parent.name.Equals(setConnections[i].parent))))
                            {
                                Debug.Log($"{setConnections[i].sceneName} - {setConnections[i].gameObject}", item.gameObject);
                                gameObjectArray.Add(item.gameObject);
                                foundIt = true;
                                break;
                            }
                        }
                        if (!foundIt)
                            Debug.Log($"Not Found, but should be here: {setConnections[i].sceneName} - {setConnections[i].gameObject} Parent: {setConnections[i].parent}");
                    }
                    if (gameObjectArray.Count != 0)
                        Selection.objects = gameObjectArray.ToArray();
                }
                else
                {
                    for (int i = 0; i < setConnections.Length; i++)
                    {
                        Debug.Log($"{setConnections[i].sceneName} - {setConnections[i].gameObject}");
                    }
                }
            }
            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                // destroy asset
                foreach (var subAsset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(serializedObject.targetObject)))
                {
                    if (subAsset == keyVariable.targetObject as UnityEngine.Object)
                    {
                        DestroyImmediate(subAsset, true);
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        AssetDatabase.SaveAssets();
                    }
                }
                if (isGlobal)
                    myTarget.globalVariables.RemoveAt(index.position);
                else
                    myTarget.persistentVariables.RemoveAt(index.position);
                EditorUtility.SetDirty(myTarget);
                //variables.DeleteArrayElementAtIndex(index.position);
                serializedObject.Update();
                return RefreshOutcome.Deleted;
            }
            EditorGUILayout.EndHorizontal();
            if (index.unfolded)
            {
                for (int j = 0; j < stateNames.arraySize; j++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.Space(15, false);
                    EditorGUILayout.PropertyField(stateNames.GetArrayElementAtIndex(j));
                    if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
                    {
                        stateNames.DeleteArrayElementAtIndex(j);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(30, false);

                if (GUILayout.Button("Add"))
                {
                    stateNames.arraySize++;
                }
                EditorGUILayout.Space(30, false);
                EditorGUILayout.EndHorizontal();
            }

            keyVariable.ApplyModifiedProperties();
            return RefreshOutcome.No;
        }
        string searchText;
        bool searching;
        void DrawSearchGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(50));
            searchText = GUILayout.TextField(searchText);
            searching = !string.IsNullOrEmpty(searchText);
            if (searching)
            {
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    searching = false;
                    searchText = "";
                }
            } else
            {
                GUILayout.Space(30);
            }
            GUILayout.EndHorizontal();
        }
        bool IsPartOfSearch(SerializedProperty variableName, SerializedProperty stateNames)
        {
            var splitText = this.searchText.Split(' ');

            foreach (var text in splitText)
            {
                if (!variableName.stringValue.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    for (int i = 0; i < stateNames.arraySize; i++)
                    {
                        bool contained = true;
                        foreach (var splitTextToTestStateNames in splitText)
                        {
                            if (!stateNames.GetArrayElementAtIndex(i).stringValue.Contains(text, StringComparison.OrdinalIgnoreCase))
                            {
                                contained = false;
                                break;
                            }
                        }
                        if (contained)
                            return true;
                    }
                    return false;
                }
            }
            return true;
        }
        public IEnumerable<GameObject> GetAllRootGameObjects()
        {
            GameObject[] allGameObjects = FindObjectsByType<GameObject>( FindObjectsInactive.Include);
            foreach (GameObject obj in allGameObjects)
            {
                yield return obj;
            }
        }
        int InScene(KeyVariable.KeyVariableConnection[] array, string sceneName)
        {
            if (array == null)
                return 0;
            int connectionsInScene = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].sceneName.Equals(sceneName))
                    connectionsInScene++;
            }
            return connectionsInScene;
        }
#endregion
#endif
}
