using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SelectableSubclassAttribute))]
public class SelectableSubclassDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        // 1. Resolve target type safely
        Type fieldType = fieldInfo.FieldType;
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            fieldType = fieldType.GetGenericArguments()[0];
        }
        else if (fieldType.IsArray)
        {
            fieldType = fieldType.GetElementType();
        }

        // 2. Scan derived sub-types
        List<Type> derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => fieldType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        var choices = new List<string> { "None" };
        choices.AddRange(derivedTypes.Select(t => t.Name));

        string GetCurrentSelectionName()
        {
            string currentTypeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(currentTypeName)) return "None";

            string cleanTypeName = currentTypeName.Split(' ').Last();
            Type currentType = derivedTypes.FirstOrDefault(t => t.FullName == cleanTypeName);
            return currentType != null ? currentType.Name : "None";
        }

        // 3. Dropdown Header
        var dropdown = new DropdownField(property.displayName, choices, GetCurrentSelectionName());

        dropdown.RegisterValueChangedCallback(evt =>
        {
            property.serializedObject.Update();

            if (evt.newValue == "None")
            {
                property.managedReferenceValue = null;
            }
            else
            {
                Type targetType = derivedTypes.FirstOrDefault(t => t.Name == evt.newValue);
                if (targetType != null)
                {
                    property.managedReferenceValue = Activator.CreateInstance(targetType);
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        });

        container.Add(dropdown);

        // 4. Child Fields Container
        var propertiesContainer = new VisualElement();
        propertiesContainer.style.paddingLeft = 15;
        container.Add(propertiesContainer);

        void UpdateChildFields()
        {
            // Unbind elements before clearing to release native GC handles cleanly
            propertiesContainer.Unbind();
            propertiesContainer.Clear();

            dropdown.SetValueWithoutNotify(GetCurrentSelectionName());

            if (property.managedReferenceValue != null)
            {
                var endProperty = property.GetEndProperty();
                var childProperty = property.Copy();

                if (childProperty.NextVisible(true))
                {
                    while (!SerializedProperty.EqualContents(childProperty, endProperty))
                    {
                        // NO MANUAL .Bind() HERE! UI Toolkit handles binding automatically.
                        var propField = new PropertyField(childProperty.Copy());
                        propertiesContainer.Add(propField);

                        if (!childProperty.NextVisible(false)) break;
                    }
                }
            }

            // Rebind the parent container cleanly
            propertiesContainer.Bind(property.serializedObject);
        }

        // 5. Track Property Modifications safely
        container.TrackPropertyValue(property, (p) => UpdateChildFields());
        UpdateChildFields();

        return container;
    }
}