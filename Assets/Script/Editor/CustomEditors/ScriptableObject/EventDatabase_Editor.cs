using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(EventDatabase))]
public class EventDatabase_Editor : Editor
{
    private string newEventName = "NewEvent";

    // Type selection caching
    private List<Type> availableEventTypes = new List<Type>();
    private string[] eventTypeNames;
    private int selectedTypeIndex = 0;

    private void OnEnable()
    {
        RefreshEventTypes();
    }

    private void RefreshEventTypes()
    {
        // Automatically find all non-abstract subclasses of BaseEvent in the project assemblies
        availableEventTypes = TypeCache.GetTypesDerivedFrom<BaseEvent>()
            .Where(t => !t.IsAbstract)
            .ToList();

        // Ensure base BaseEvent is included at the top if not already caught
        if (!availableEventTypes.Contains(typeof(BaseEvent)))
        {
            availableEventTypes.Insert(0, typeof(BaseEvent));
        }

        // Create a friendly string array for the popup dropdown
        eventTypeNames = availableEventTypes.Select(t => t.Name).ToArray();
    }

    public override void OnInspectorGUI()
    {
        EventDatabase database = (EventDatabase)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Master Event Database", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create and manage any type of BaseEvent sub-asset here.", MessageType.Info);
        EditorGUILayout.Space();

        // --- CREATION SECTION ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Create New Event", EditorStyles.miniBoldLabel);

        newEventName = EditorGUILayout.TextField("Event Name", newEventName);

        if (eventTypeNames != null && eventTypeNames.Length > 0)
        {
            selectedTypeIndex = EditorGUILayout.Popup("Event Type", selectedTypeIndex, eventTypeNames);
        }

        if (GUILayout.Button("Create Event Sub-Asset", GUILayout.Height(30)))
        {
            if (!string.IsNullOrEmpty(newEventName) && availableEventTypes.Count > 0)
            {
                // 1. Get the chosen Type from our discovered list
                Type chosenType = availableEventTypes[selectedTypeIndex];

                // 2. Dynamically instantiate that specific class type
                BaseEvent newEvent = (BaseEvent)CreateInstance(chosenType);
                newEvent.name = newEventName;

                // 3. Add to database list
                Undo.RecordObject(database, "Add Event");
                database.events.Add(newEvent);

                // 4. Attach as a sub-asset to the master database asset file
                AssetDatabase.AddObjectToAsset(newEvent, database);

                // 5. Save changes
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                newEventName = "NewEvent";
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // --- LIST SECTION ---
        EditorGUILayout.LabelField($"Registered Events ({database.events.Count})", EditorStyles.boldLabel);

        if (database.events.Count == 0)
        {
            EditorGUILayout.LabelField("No events created yet.", EditorStyles.miniLabel);
            return;
        }

        // Clean up any null entries
        database.events.RemoveAll(item => item == null);

        for (int i = database.events.Count - 1; i >= 0; i--)
        {
            BaseEvent evt = database.events[i];
            if (evt == null) continue;

            EditorGUILayout.BeginHorizontal("box");

            // Display object field and its actual concrete type name
            EditorGUILayout.ObjectField(evt, typeof(BaseEvent), false);

            // Show type badge on the right
            GUIStyle typeLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            EditorGUILayout.LabelField($"({evt.GetType().Name})", typeLabelStyle, GUILayout.Width(100));

            // Delete button
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Event", $"Are you sure you want to delete event '{evt.name}'?", "Yes", "No"))
                {
                    Undo.RecordObject(database, "Delete Event");
                    database.events.RemoveAt(i);

                    Undo.DestroyObjectImmediate(evt);

                    EditorUtility.SetDirty(database);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}
