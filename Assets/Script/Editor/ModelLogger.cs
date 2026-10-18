using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class ModelLogger
{
    /// <summary>
    /// Generates the exact same formatted string used in logs, without sending it to the Unity Console.
    /// </summary>
    public static string GetModelString(object model)
    {
        if (model == null) return "Model is null";

        Type type = model.GetType();
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"<b>Dump of {type.Name}</b>");
        sb.AppendLine("------------------------------------------------");

        // Fields
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sb.AppendLine("<b>[Fields]</b>");
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(model);
            var attr = field.GetCustomAttribute<CardModelTypeAttribute>();
            Type cardTypeContext = attr?.CardType;
            sb.AppendLine($"  {field.Name}: {FormatValue(value, cardTypeContext)}");
        }

        sb.AppendLine("------------------------------------------------");

        // Properties
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sb.AppendLine("<b>[Properties]</b>");
        foreach (PropertyInfo property in properties)
        {
            if (property.GetIndexParameters().Length > 0) continue;
            try
            {
                object value = property.GetValue(model, null);
                var attr = property.GetCustomAttribute<CardModelTypeAttribute>();
                Type cardTypeContext = attr?.CardType;
                sb.AppendLine($"  {property.Name}: {FormatValue(value, cardTypeContext)}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  {property.Name}: <color=red>Error reading ({ex.Message})</color>");
            }
        }

        return sb.ToString();
    }
    /// <summary>
    /// Inspects and logs all fields and properties of any given object, including collections.
    /// </summary>
    public static void LogModelState(object model, string prefix = "MODEL STATE")
    {
        if (model == null)
        {
            Debug.LogWarning($"[{prefix}] Cannot log state because the model is null.");
            return;
        }

        Type type = model.GetType();
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"<b>[{prefix}] Dump of {type.Name}</b>");
        sb.AppendLine("------------------------------------------------");

        // 1. Fetch all instance fields
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sb.AppendLine("<b>[Fields]</b>");
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(model);
            var attr = field.GetCustomAttribute<CardModelTypeAttribute>();
            Type cardTypeContext = attr?.CardType;
            sb.AppendLine($"  {field.Name}: {FormatValue(value, cardTypeContext)}");
        }

        sb.AppendLine("------------------------------------------------");

        // 2. Fetch all instance properties
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sb.AppendLine("<b>[Properties]</b>");
        foreach (PropertyInfo property in properties)
        {
            if (property.GetIndexParameters().Length > 0) continue;

            try
            {
                object value = property.GetValue(model, null);
                var attr = property.GetCustomAttribute<CardModelTypeAttribute>();
                Type cardTypeContext = attr?.CardType;
                sb.AppendLine($"  {property.Name}: {FormatValue(value, cardTypeContext)}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  {property.Name}: <color=red>Error reading ({ex.Message})</color>");
            }
        }

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Smart formatting that unwraps Dictionaries, Lists, Arrays, and primitives.
    /// </summary>
    private static string FormatValue(object value, Type cardTypeContext, int indentLevel = 1)
    {
        if (value == null) return "<color=#7F8C8D>null</color>";

        Type type = value.GetType();

        // 1. Handle Strings (Strings are IEnumerable, so check them explicitly first)
        if (type == typeof(string))
        {
            string stringValue = (string)value;
            string formatted = $"<color=#E67E22>\"{stringValue}\"</color>";

            if (cardTypeContext != null && !string.IsNullOrEmpty(stringValue))
            {
                ICardModel cardModel = ResolveCardFromRegistry(stringValue, cardTypeContext);
                if (cardModel != null)
                {
                    // Replace 'CardName' with whatever shared property your ICardModel interface uses
                    formatted += $" <color=#7F8C8D>// Card: {cardModel}</color>";
                }
            }

            return formatted;
        }

        // 2. Handle Primitives, Enums, and common Structs directly
        if (type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(Guid))
        {
            return $"<color=#1794BB>{value}</color>";
        }

        // Create an indentation string based on recursion depth
        string indent = new string(' ', indentLevel * 2);
        string subIndent = new string(' ', (indentLevel + 1) * 2);

        // 3. Handle Dictionaries
        if (value is IDictionary dictionary)
        {
            if (dictionary.Count == 0) return "<color=#7F8C8D>Dictionary (Empty)</color>";

            StringBuilder dictSb = new StringBuilder();
            dictSb.AppendLine($"<color=#AF38EB>Dictionary<{GetGenericArgumentsString(type)}></color> (Count: {dictionary.Count})");
            dictSb.AppendLine($"{indent}{{");

            foreach (DictionaryEntry entry in dictionary)
            {
                dictSb.AppendLine($"{subIndent}[{FormatValue(entry.Key, cardTypeContext, indentLevel + 1)}] => {FormatValue(entry.Value, cardTypeContext, indentLevel + 1)}");
            }

            dictSb.Append($"{indent}}}");
            return dictSb.ToString();
        }

        // 4. Handle Lists and Arrays (and other IEnumerables)
        if (value is IEnumerable enumerable)
        {
            // Gather collection elements to find total count
            var list = new ArrayList();
            foreach (var item in enumerable) list.Add(item);

            if (list.Count == 0) return $"<color=#7F8C8D>{type.Name} (Empty)</color>";

            StringBuilder listSb = new StringBuilder();
            string cleanTypeName = type.IsArray ? $"{type.GetElementType().Name}[]" : $"{type.Name}<{GetGenericArgumentsString(type)}>";

            listSb.AppendLine($"<color=#2ECC71>{cleanTypeName}</color> (Count: {list.Count})");
            listSb.AppendLine($"{indent}[");

            for (int i = 0; i < list.Count; i++)
            {
                listSb.AppendLine($"{subIndent}Index {i}: {FormatValue(list[i], cardTypeContext, indentLevel + 1)}");
            }

            listSb.Append($"{indent}]");
            return listSb.ToString();
        }

        // 5. Fallback for custom nested C# classes/structs inside the model
        return $"<color=#95A5A6>({type.Name})</color> {value}";
    }

    private static ICardModel ResolveCardFromRegistry(string cardId, Type targetCardType)
    {
        try
        {
            // 1. Get the generic method info for GetModel<T>
            MethodInfo method = typeof(CardRegistryModel).GetMethod(
                nameof(CardRegistryModel.GetModel),
                BindingFlags.Public | BindingFlags.Static
            );

            if (method == null) return null;

            // 2. Supply the generic argument (e.g. SpellCardModel, CreatureCardModel)
            MethodInfo genericMethod = method.MakeGenericMethod(targetCardType);

            // 3. Invoke the static method: CardRegistryModel.GetModel<targetCardType>(cardId)
            return genericMethod.Invoke(null, new object[] { cardId }) as ICardModel;
        }
        catch
        {
            // Fail silently if the ID doesn't exist or isn't initialized yet
            return null;
        }
    }

    /// <summary>
    /// Helper to get clean generic argument names for logging (e.g., "String, Int32")
    /// </summary>
    private static string GetGenericArgumentsString(Type type)
    {
        if (!type.IsGenericType) return "";
        Type[] args = type.GetGenericArguments();
        string[] names = new string[args.Length];
        for (int i = 0; i < args.Length; i++) names[i] = args[i].Name;
        return string.Join(", ", names);
    }
}