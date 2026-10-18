using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

public class TermManagerWindow : EditorWindow
{
    static TermManagerWindow window;
    private TooltipTermsData database;
    private ToolTipTerm selectedTerm;
    private Editor termEditor;

    private Vector2 sidebarScroll;
    private Vector2 mainScroll;
    private string searchFilter = "";
    private string newTermID = "";
    private LocalizedString newDisplayName;

    TempLocalizedStringContainer tempLocalizedStringContainer;
    SerializedObject tempLocalizedStringSerilizedObject;
    SerializedProperty tempLocalizedStringSerilizedProperty;
    [MenuItem("Tools/Terms Manager")]
    public static void OpenWindow()
    {
        window = GetWindow<TermManagerWindow>("Terms Manager");
        window.tempLocalizedStringContainer = ScriptableObject.CreateInstance<TempLocalizedStringContainer>();
    }
    private void OnEnable()
    {
        InitTempContainer();
    }
    private void InitTempContainer()
    {
        if (tempLocalizedStringContainer == null)
        {
            newDisplayName = new LocalizedString();
            tempLocalizedStringContainer = ScriptableObject.CreateInstance<TempLocalizedStringContainer>();
            tempLocalizedStringContainer.localizedString = newDisplayName;
        }

        tempLocalizedStringSerilizedObject = new SerializedObject(tempLocalizedStringContainer);
        tempLocalizedStringSerilizedProperty = tempLocalizedStringSerilizedObject.FindProperty(nameof(TempLocalizedStringContainer.localizedString));
    }

    private void OnGUI()
    {
        // Auto-assign database if null
        if (database == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TooltipTermsData");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                database = AssetDatabase.LoadAssetAtPath<TooltipTermsData>(path);
            }
        }

        database = (TooltipTermsData)EditorGUILayout.ObjectField("Database Asset", database, typeof(TooltipTermsData), false);

        if (database == null)
        {
            EditorGUILayout.HelpBox("Please assign or create a TermsDatabaseSO asset.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(5);

        // Main 2-Column Layout
        EditorGUILayout.BeginHorizontal();

        // --------------------------------------------------
        // LEFT COLUMN: Sidebar (Term Creation & List)
        // --------------------------------------------------
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250), GUILayout.ExpandHeight(true));

        // --- Create New Term UI ---
        GUILayout.Label("Create New Term", EditorStyles.boldLabel);
        newTermID = EditorGUILayout.TextField("Term ID", newTermID);

        // Draw LocalizedString selector UI for creation
        EditorGUILayout.LabelField("Display Name (Localized)", EditorStyles.miniBoldLabel);
        DrawLocalizedStringDrawer();

        if (GUILayout.Button("Add Term", GUILayout.Height(25)))
        {
            if (string.IsNullOrEmpty(newTermID))
            {
                EditorUtility.DisplayDialog("Error", "Term ID cannot be empty.", "OK");
            }
            else if (database.GetTerm(newTermID) != null)
            {
                EditorUtility.DisplayDialog("Error", $"Term ID '{newTermID}' already exists!", "OK");
            }
            else
            {
                selectedTerm = database.CreateNewTerm(newTermID, newDisplayName);

                //Reset
                newTermID = "";
                newDisplayName = new LocalizedString();
                GUI.FocusControl(null);
            }
        }

        EditorGUILayout.Space(10);
        searchFilter = EditorGUILayout.TextField("Search", searchFilter);
        EditorGUILayout.Space(5);

        // --- List of Existing Terms ---
        sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);
        foreach (var term in database.AllTerms)
        {
            if (term == null) continue;

            string previewName = GetLocalizedStringPreview(term.defaultDisplayName, term.termID);
            // Search filter check
            if (!string.IsNullOrEmpty(searchFilter) &&
                !term.termID.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) &&
                !previewName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            GUI.backgroundColor = (selectedTerm == term) ? new Color(0.3f, 0.6f, 1f) : Color.white;

            if (GUILayout.Button($"{previewName} ({term.termID})", EditorStyles.miniButton, GUILayout.Height(22)))
            {
                selectedTerm = term;
                // Force active editor recreation when switching terms
                if (termEditor != null) DestroyImmediate(termEditor);
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        // --------------------------------------------------
        // RIGHT COLUMN: Inspector View for Selected Term
        // --------------------------------------------------
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (selectedTerm != null)
        {
            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Editing: {selectedTerm.termID}", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete Term", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", $"Delete term '{selectedTerm.termID}'?", "Yes", "No"))
                {
                    database.DeleteTerm(selectedTerm);
                    selectedTerm = null;
                    if (termEditor != null) DestroyImmediate(termEditor);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Draw default inspector for the selected ScriptableObject sub-asset
            if (termEditor == null || termEditor.target != selectedTerm)
            {
                termEditor = Editor.CreateEditor(selectedTerm);
            }

            termEditor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("Select a term from the left sidebar to edit details.", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    private void DrawLocalizedStringDrawer()
    {
        tempLocalizedStringSerilizedObject.Update();
        EditorGUILayout.PropertyField(tempLocalizedStringSerilizedProperty, GUIContent.none, true);
        tempLocalizedStringSerilizedObject.ApplyModifiedProperties();
    }

    private string GetLocalizedStringPreview(LocalizedString localizedString, string fallback)
    {
        if (localizedString != null && !localizedString.IsEmpty)
        {
            string entry = localizedString.GetLocalizedString();
            if (!string.IsNullOrEmpty(entry)) return entry;
        }
        return fallback;
    }

    // Dummy container used solely for rendering PropertyField of standalone LocalizedString
    private class TempLocalizedStringContainer : ScriptableObject
    {
        public LocalizedString localizedString;
    }
}
