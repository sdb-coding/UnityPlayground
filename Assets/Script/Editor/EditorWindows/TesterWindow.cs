using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class TesterWindow : EditorWindow
{
    [MenuItem("Tools/TestingWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TesterWindow));
    }
    void OnGUI()
    {
        if (GUILayout.Button("Spawn Ships"))
        {
            
        }
    }
}
