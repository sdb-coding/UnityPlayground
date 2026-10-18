using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using System;

[CreateAssetMenu(fileName = "HierarchyEditorData", menuName = "EditorData/HierarchyEditorData")]
public class HierarchyEditorData : ScriptableObject
{
    [CustomEditor(typeof(HierarchyEditorData))]
    public class HierarchyEditorDataEditor : Editor
    {
        public struct TextureObject
        {
            public Texture2D texture;
            public string name;
        }
        List<TextureObject> editorIcons;
        private void ExportIcons()
        {
            editorIcons = new List<TextureObject>();
            var editorAssetBundle = GetEditorAssetBundle();
            var iconsPath = GetIconsPath();
            var count = 0;
            foreach (var assetName in EnumerateIcons(editorAssetBundle, iconsPath))
            {
                var icon = editorAssetBundle.LoadAsset<Texture2D>(assetName);
                if (icon == null)
                    continue;

                var readableTexture = new Texture2D(icon.width, icon.height, icon.format, icon.mipmapCount > 1);
                Graphics.CopyTexture(icon,readableTexture);
                editorIcons.Add(new TextureObject()
                {
                    name = assetName,
                    texture = readableTexture
                });


                count++;
            }

            Debug.Log($"{count} icons has been found!");
        }
        private static IEnumerable<string> EnumerateIcons(AssetBundle editorAssetBundle, string iconsPath)
        {
            foreach (var assetName in editorAssetBundle.GetAllAssetNames())
            {
                if (assetName.StartsWith(iconsPath, StringComparison.OrdinalIgnoreCase) == false)
                    continue;
                if (assetName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false &&
                    assetName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                yield return assetName;
            }
        }
        private static AssetBundle GetEditorAssetBundle()
        {
            var editorGUIUtility = typeof(EditorGUIUtility);
            var getEditorAssetBundle = editorGUIUtility.GetMethod(
                "GetEditorAssetBundle",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (AssetBundle)getEditorAssetBundle.Invoke(null, new object[] { });
        }
        private static string GetIconsPath()
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.EditorResources.iconsPath;
#else
            var assembly = typeof(EditorGUIUtility).Assembly;
            var editorResourcesUtility = assembly.GetType("UnityEditorInternal.EditorResourcesUtility");

            var iconsPathProperty = editorResourcesUtility.GetProperty(
                "iconsPath",
                BindingFlags.Static | BindingFlags.Public);

            return (string)iconsPathProperty.GetValue(null, new object[] { });
#endif
        }
        private void OnEnable()
        {
            ExportIcons();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var previousSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(20,20));
            EditorGUILayout.LabelField("TextureIcons");
            float width = EditorGUIUtility.currentViewWidth;
            int columns = Mathf.FloorToInt( width / 40);
            int column = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var item in editorIcons)
            {
                if (column == columns)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    column = 0;
                }
                column++;
                DisplayTexture(item);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.SetIconSize(previousSize);
        }
        void DisplayTexture(TextureObject textureObject)
        {
            if (GUILayout.Button(textureObject.texture))
            {
                GUIUtility.systemCopyBuffer = textureObject.name;
            }
        }
    }
    public enum FilterType {All, Name, Type }
    [System.Serializable]
    public class Filter
    {
        public string description;
        public string filter;
        public string editorIcon;
        public FilterType filterType;
    }
    [System.Serializable]
    public class FilterPresets
    {
        public string presetName;
        public Filter[] filters;
    }
    public Vector2 iconSize = Vector2.one;
    public FilterPresets[] filterPresets;
}
