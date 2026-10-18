using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class SaveRunner : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SaveRunner))]
    public class SaveRunner_Editor : Editor
    {
        SaveRunner myTarget;
        private void OnEnable()
        {
            myTarget = (SaveRunner)target;
        }
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Save Game"))
                {
                    myTarget.SaveCurrentGame();
                }
                if (GUILayout.Button("Load Game"))
                {
                    myTarget.LoadDefaultGame();
                }
            }
            base.OnInspectorGUI();
        }
    }
#endif

    private void LoadDefaultGame()
    {
        Services.SaveSystem.SaveLoadManager.LoadActiveRun();
    }

    private void SaveCurrentGame()
    {
        Services.SaveSystem.SaveLoadManager.SaveRun();
    }
}
