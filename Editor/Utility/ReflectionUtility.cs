using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace THEBADDEST.EditorTools
{
    public static class ReflectionUtility
    {
        // Caching dictionaries to avoid repeated reflection calls
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> _fieldCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> _methodCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        private static readonly Dictionary<Type, List<Type>> _typeHierarchyCache = new Dictionary<Type, List<Type>>();

        public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(object target, Func<PropertyInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<PropertyInfo> propertyInfos = types[i]
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }

        public static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<MethodInfo> methodInfos = types[i]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }

        public static FieldInfo GetField(object target, string fieldName)
        {
            if (target == null)
            {
                return null;
            }

            Type targetType = target.GetType();
            
            // Check cache first
            if (!_fieldCache.TryGetValue(targetType, out Dictionary<string, FieldInfo> typeFields))
            {
                typeFields = new Dictionary<string, FieldInfo>();
                _fieldCache[targetType] = typeFields;
            }
            else if (typeFields.TryGetValue(fieldName, out FieldInfo cachedField))
            {
                return cachedField;
            }

            // Not in cache, search through type hierarchy
            List<Type> types = GetSelfAndBaseTypes(target);
            for (int i = types.Count - 1; i >= 0; i--)
            {
                FieldInfo field = types[i].GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                {
                    typeFields[fieldName] = field;
                    return field;
                }
            }

            // Not found, cache null result to avoid repeated searches
            typeFields[fieldName] = null;
            return null;
        }

        public static PropertyInfo GetProperty(object target, string propertyName)
        {
            if (target == null)
            {
                return null;
            }

            Type targetType = target.GetType();
            
            // Check cache first
            if (!_propertyCache.TryGetValue(targetType, out Dictionary<string, PropertyInfo> typeProperties))
            {
                typeProperties = new Dictionary<string, PropertyInfo>();
                _propertyCache[targetType] = typeProperties;
            }
            else if (typeProperties.TryGetValue(propertyName, out PropertyInfo cachedProperty))
            {
                return cachedProperty;
            }

            // Not in cache, search through type hierarchy
            List<Type> types = GetSelfAndBaseTypes(target);
            for (int i = types.Count - 1; i >= 0; i--)
            {
                PropertyInfo property = types[i].GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    typeProperties[propertyName] = property;
                    return property;
                }
            }

            // Not found, cache null result to avoid repeated searches
            typeProperties[propertyName] = null;
            return null;
        }

        public static MethodInfo GetMethod(object target, string methodName)
        {
            if (target == null)
            {
                return null;
            }

            Type targetType = target.GetType();
            
            // Check cache first
            if (!_methodCache.TryGetValue(targetType, out Dictionary<string, MethodInfo> typeMethods))
            {
                typeMethods = new Dictionary<string, MethodInfo>();
                _methodCache[targetType] = typeMethods;
            }
            else if (typeMethods.TryGetValue(methodName, out MethodInfo cachedMethod))
            {
                return cachedMethod;
            }

            // Not in cache, search through type hierarchy
            List<Type> types = GetSelfAndBaseTypes(target);
            for (int i = types.Count - 1; i >= 0; i--)
            {
                MethodInfo method = types[i].GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    typeMethods[methodName] = method;
                    return method;
                }
            }

            // Not found, cache null result to avoid repeated searches
            typeMethods[methodName] = null;
            return null;
        }

        public static Type GetListElementType(Type listType)
        {
            if (listType.IsGenericType)
            {
                return listType.GetGenericArguments()[0];
            }
            else
            {
                return listType.GetElementType();
            }
        }

        /// <summary>
        ///		Get type and all base types of target, sorted as following:
        ///		<para />[target's type, base type, base's base type, ...]
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            Type targetType = target.GetType();
            
            // Check cache
            if (_typeHierarchyCache.TryGetValue(targetType, out List<Type> cachedTypes))
            {
                return cachedTypes;
            }

            // Build type hierarchy
            List<Type> types = new List<Type> { targetType };
            Type currentType = targetType.BaseType;
            
            while (currentType != null)
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }

            // Cache the result
            _typeHierarchyCache[targetType] = types;
            return types;
        }

        /// <summary>
        /// Clears all reflection caches. Call this when assemblies are reloaded.
        /// </summary>
        public static void ClearCache()
        {
            _fieldCache.Clear();
            _propertyCache.Clear();
            _methodCache.Clear();
            _typeHierarchyCache.Clear();
        }
    }
}
