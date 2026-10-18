using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Newtonsoft.Json;
public static class TypeExtension 
{
    public static List<Type> GetAllDerivedClasses(this Type aBaseClass, string[] aExcludeAssemblies)
    {
        List<Type> result = new List<Type>();
        foreach (Assembly A in AppDomain.CurrentDomain.GetAssemblies())
        {
            bool exclude = false;
            foreach (string S in aExcludeAssemblies)
            {
                if (A.GetName().FullName.StartsWith(S))
                {
                    exclude = true;
                    break;
                }
            }
            if (exclude)
                continue;
            if (aBaseClass.IsInterface)
            {
                foreach (Type C in A.GetExportedTypes())
                    foreach (Type I in C.GetInterfaces())
                        if (aBaseClass == I)
                        {
                            result.Add(C);
                            break;
                        }
            }
            else
            {
                foreach (Type C in A.GetExportedTypes())
                    if (C.IsSubclassOf(aBaseClass))
                    {
                        result.Add(C);
                    }
            }
        }
        return result;
    }

    public static List<Type> GetAllDerivedClasses(this Type aBaseClass)
    {
        return new List<Type>(System.AppDomain.CurrentDomain.GetAllDerivedTypes(aBaseClass));
        //return GetAllDerivedClasses(aBaseClass, new string[0]);
    }

    public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
    {
        var result = new List<System.Type>();
        var assemblies = aAppDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(aType))
                    result.Add(type);
            }
        }
        return result.ToArray();
    }
    public static System.Type[] GetAllDerivedTypes<T>(this System.AppDomain aAppDomain)
    {
        return GetAllDerivedTypes(aAppDomain, typeof(T));
    }
}
[Serializable]
public class RefEvent<T>
{
    public RefEvent()
    {
        reference = default;
    }
    public RefEvent(T t)
    {
        reference = t;
    }
    public event Action<RefEvent<T>> onChange;
    public T Value
    {
        get => reference; set
        {
            if (!object.Equals(reference, value))
            {
                reference = value;
                if (onChange != null)
                    onChange(this);
            }
        }
    }
    [JsonIgnore] public bool isNull => reference == null;
    [SaveState][SerializeField] T reference;
    public static implicit operator T(RefEvent<T> refEvent) => refEvent != null ? refEvent.Value : default;
    public void Set(T newValue) => Value = newValue;
    public override string ToString()
    {
        return reference != null ? reference.ToString() : "null";
    }
}
    public class RefEventConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(RefEvent<>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var valueProperty = value.GetType().GetProperty("Value");
            object innerValue = valueProperty?.GetValue(value);

            serializer.Serialize(writer, innerValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (existingValue == null)
            {
                existingValue = Activator.CreateInstance(objectType);
            }

            Type valueType = objectType.GetGenericArguments()[0];
            object deserializedValue = serializer.Deserialize(reader, valueType);

            var setMethod = objectType.GetMethod("Set");
            setMethod?.Invoke(existingValue, new[] { deserializedValue });

            return existingValue;
        }
    }
