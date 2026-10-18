using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace Services.SaveSystem
{
    public static class ReflectionSaveEngine
    {
        #region JsonConverters
        private static readonly List<JsonConverter> RegisteredConverters = new();
        public static JsonSerializerSettings _cachedSettings;
        private static JsonSerializer _cachedSerializer;

        static ReflectionSaveEngine()
        {
            // Auto-discover all JsonConverter implementations in the project at startup
            AutoRegisterConverters();
        }

        /// <summary>
        /// Automatically scans assemblies and adds any non-abstract JsonConverter to the engine.
        /// </summary>
        public static void AutoRegisterConverters()
        {
            RegisteredConverters.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                string name = assembly.FullName;
                // Skip system, Unity, and Newtonsoft assemblies for performance and safety
                if (name.StartsWith("Unity") || name.StartsWith("System") ||
                    name.StartsWith("mscorlib") || name.StartsWith("Newtonsoft") ||
                    name.StartsWith("Mono"))
                    continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        //if (type.ToString() == "RefEventConverter")
                        //{
                        //    Debug.Log($"{type.IsAbstract} - {type.IsInterface} {type.ContainsGenericParameters} - {typeof(JsonConverter).IsAssignableFrom(type)}");
                        //}
                        //Debug.Log(type.ToString());
                        // Must inherit from JsonConverter, not be abstract/interface, and CANNOT be an open generic definition
                        if (typeof(JsonConverter).IsAssignableFrom(type) &&
                            !type.IsAbstract &&
                            !type.IsInterface &&
                            !type.ContainsGenericParameters) // <--- CRITICAL FIX
                        {
                            // Ensure a parameterless public constructor exists
                            var constructor = type.GetConstructor(Type.EmptyTypes);
                            if (constructor != null)
                            {
                                var converter = (JsonConverter)Activator.CreateInstance(type);
                                RegisterConverter(converter);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    // Ignore assemblies or types that fail to load reflection info dynamically
                }
            }

            RebuildSerializer();
        }
        public static void RegisterConverter(JsonConverter converter)
        {
            if (converter == null) return;

            Type converterType = converter.GetType();
            if (!RegisteredConverters.Exists(c => c.GetType() == converterType))
            {
                RegisteredConverters.Add(converter);
                RebuildSerializer();
            }
        }

        private static void RebuildSerializer()
        {
            _cachedSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>(RegisteredConverters),
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) =>
                {
                    Debug.LogError($"[SaveSystem DETECTOR] Error at Path: '{args.ErrorContext.Path}' | Member: '{args.ErrorContext.Member}'");
                    Debug.LogError($"[SaveSystem DETECTOR] Exception: {args.ErrorContext.Error.Message}");
                }
            };
            _cachedSerializer = JsonSerializer.Create(_cachedSettings);
        }
        #endregion
        #region ExtractAndRestoringState
        private static readonly Dictionary<Type, List<FieldInfo>> FieldCache = new();

        private static List<FieldInfo> GetSaveFields(Type type)
        {
            if (FieldCache.TryGetValue(type, out var cached)) return cached;

            var list = new List<FieldInfo>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
                    continue;
                if (Attribute.IsDefined(field, typeof(SaveStateAttribute)))
                    list.Add(field);
            }

            FieldCache[type] = list;
            return list;
        }

        public static Dictionary<string, object> ExtractState(object target, params JsonConverter[] customConverters)
        {
            if (target == null) return null;
            if (target is UnityEngine.Object unityObj)
            {
                Debug.Log(target);
            }
            // If runtime converters were explicitly passed, ensure they are registered
            if (customConverters != null && customConverters.Length > 0)
            {
                foreach (var conv in customConverters) RegisterConverter(conv);
            }

            var state = new Dictionary<string, object>();
            foreach (var field in GetSaveFields(target.GetType()))
            {
                state[field.Name] = field.GetValue(target);
                Debug.Log($"{field.Name} added");
            }

            return state;
        }

        public static void RestoreState(object target, Dictionary<string, object> state, params JsonConverter[] customConverters)
        {
            if (target == null || state == null) return;

            if (customConverters != null && customConverters.Length > 0)
            {
                foreach (var conv in customConverters) RegisterConverter(conv);
            }

            foreach (var field in GetSaveFields(target.GetType()))
            {
                if (state.TryGetValue(field.Name, out object rawValue))
                {
                    try
                    {
                        Type fieldType = field.FieldType;
                        object existingFieldValue = field.GetValue(target);

                        bool isRefEvent = fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(RefEvent<>);

                        if (rawValue is JToken jToken)
                        {
                            // Deserializes using the dynamically maintained serializer containing all converters
                            object deserializedObj = jToken.ToObject(fieldType, _cachedSerializer);

                            if (isRefEvent && existingFieldValue != null)
                            {
                                var valueProp = fieldType.GetProperty("Value");
                                object innerVal = valueProp?.GetValue(deserializedObj);

                                var setMethod = fieldType.GetMethod("Set");
                                setMethod?.Invoke(existingFieldValue, new[] { innerVal });
                            }
                            else
                            {
                                field.SetValue(target, deserializedObj);
                            }
                        }
                        else if (isRefEvent && existingFieldValue != null)
                        {
                            var setMethod = fieldType.GetMethod("Set");
                            object converted = Convert.ChangeType(rawValue, fieldType.GetGenericArguments()[0]);
                            setMethod?.Invoke(existingFieldValue, new[] { converted });
                        }
                        else if (fieldType.IsEnum)
                        {
                            if (rawValue is IConvertible)
                            {
                                object enumValue = Enum.ToObject(fieldType, Convert.ToInt64(rawValue));
                                field.SetValue(target, enumValue);
                            }

                            // 2. If value is stored as a string name ("Store")
                            if (rawValue is string stringVal)
                            {
                                object enumValue = Enum.Parse(fieldType, stringVal);
                                field.SetValue(target, enumValue);
                            }
                        }
                        else
                        {
                            object converted = Convert.ChangeType(rawValue, fieldType);
                            field.SetValue(target, converted);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SaveEngine] Failed to restore field '{field.Name}' on {target.GetType()}: {e.Message}");
                    }
                }
            }
        }
        #endregion
    }
}