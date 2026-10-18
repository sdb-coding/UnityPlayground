using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[CreateAssetMenu(fileName ="KeysAsset", menuName = "Data/KeysAsset")]
public class KeysAsset : ScriptableObject
{
    public class DictionaryOfKeyValues : SerializedDictionary<KeyVariable, int> { }

    public List<KeyVariable> persistentVariables = new List<KeyVariable>();
    public List<KeyVariable> globalVariables = new List<KeyVariable>();


#if DEBUG
        public bool sortedView;
        public List<Parent> globalParents = new List<Parent>();
        public List<Parent> persistentParents = new List<Parent>();
        
        [System.Serializable]
        public class Parent
        {
            public string parentName;
            public bool unfolded;
            public bool important;
        }
        public bool showUnparentedGlobal;
        public bool showUnparentedPersistent;
        public Parent GetPersistentParentFromName(string parentName)
        {
            foreach (var item in persistentParents)
            {
                if (item.parentName.Equals(parentName))
                    return item;
            }
            return null;
        }
        public Parent GetGlobalParentFromName(string parentName)
        {
            foreach (var item in globalParents)
            {
                if (item.parentName.Equals(parentName))
                    return item;
            }
            return null;
        }
#endif
}
