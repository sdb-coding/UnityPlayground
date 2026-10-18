using UnityEditor;
using UnityEngine;
using System;

[InitializeOnLoad]
public static class ModelInspectorInjector
{
    private static bool _showDebugFoldout = true;
    private static Vector2 _scrollPosition;
    private static GUIStyle _textAreaStyle;

    // Static constructor registers the event automatically when the Editor loads
    static ModelInspectorInjector()
    {
        Editor.finishedDefaultHeaderGUI -= OnPostInspectorGUI;
        Editor.finishedDefaultHeaderGUI += OnPostInspectorGUI;
    }

    private static void OnPostInspectorGUI(Editor editor)
    {
        // 1. Check if the target object is a MonoBehaviour component
        GameObject selectedGameObject = editor.target as GameObject;
        if (selectedGameObject == null) return;

        // 2. Check if this GameObject has a component implementing your interface
        IModelContainer container = selectedGameObject.GetComponent<IModelContainer>();
        if (container == null) return;

        // 3. Render a clean visual line separator
        EditorGUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1.0f));
        EditorGUILayout.Space(5);

        // 4. Render the Collapsible Foldout Header
        _showDebugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugFoldout, "MVP Model Live State");

        if (_showDebugFoldout)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see the active Model state.", MessageType.Info);
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            object model = container.GetModel();
            if (model == null)
            {
                EditorGUILayout.HelpBox("The Model instance is currently null.", MessageType.Warning);
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            // Setup styling for the box
            if (_textAreaStyle == null)
            {
                _textAreaStyle = new GUIStyle(EditorStyles.textArea)
                {
                    richText = true,
                    fontStyle = FontStyle.Normal,
                    wordWrap = false
                };
            }

            // 5. Fetch and draw the model properties
            string modelDump = ModelLogger.GetModelString(model);
            float calculatedHeight = Mathf.Min(400, Mathf.Max(100, modelDump.Split('\n').Length * 16));

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(calculatedHeight));
            EditorGUILayout.TextArea(modelDump, _textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            // Force the specific open inspector to refresh every frame while playing
            editor.Repaint();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
