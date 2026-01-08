using UnityEditor;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.EditorTools
{


	public static class PropertyUtility
	{

		// Caching for attributes and field info per property path
		private static readonly Dictionary<string, Dictionary<Type, object[]>> _attributeCache = new Dictionary<string, Dictionary<Type, object[]>>();
		private static readonly Dictionary<string, FieldInfo> _fieldInfoCache = new Dictionary<string, FieldInfo>();
		private static readonly Dictionary<string, object> _targetObjectCache = new Dictionary<string, object>();
		private static readonly Dictionary<string, GUIContent> _labelCache = new Dictionary<string, GUIContent>();

		private static string GetPropertyCacheKey(SerializedProperty property)
		{
			return property.serializedObject.targetObject.GetInstanceID() + "." + property.propertyPath;
		}

		public static T GetAttribute<T>(SerializedProperty property) where T : class
		{
			T[] attributes = GetAttributes<T>(property);
			return (attributes.Length > 0) ? attributes[0] : null;
		}

		public static T[] GetAttributes<T>(SerializedProperty property) where T : class
		{
			string cacheKey = GetPropertyCacheKey(property);
			Type attributeType = typeof(T);

			// Check attribute cache
			if (_attributeCache.TryGetValue(cacheKey, out Dictionary<Type, object[]> typeAttributes))
			{
				if (typeAttributes.TryGetValue(attributeType, out object[] cachedAttributes))
				{
					return (T[])cachedAttributes;
				}
			}
			else
			{
				typeAttributes = new Dictionary<Type, object[]>();
				_attributeCache[cacheKey] = typeAttributes;
			}

			// Get field info (with caching)
			FieldInfo fieldInfo = GetCachedFieldInfo(property);
			if (fieldInfo == null)
			{
				var empty = new T[0];
				typeAttributes[attributeType] = empty;
				return empty;
			}

			// Get attributes and cache them
			object[] attributes = fieldInfo.GetCustomAttributes(attributeType, true);
			typeAttributes[attributeType] = attributes;
			return (T[])attributes;
		}

		private static FieldInfo GetCachedFieldInfo(SerializedProperty property)
		{
			string cacheKey = GetPropertyCacheKey(property);
			if (_fieldInfoCache.TryGetValue(cacheKey, out FieldInfo cachedFieldInfo))
			{
				return cachedFieldInfo;
			}

			object target = GetTargetObjectWithProperty(property);
			FieldInfo fieldInfo = ReflectionUtility.GetField(target, property.name);
			_fieldInfoCache[cacheKey] = fieldInfo;
			return fieldInfo;
		}

		public static GUIContent GetLabel(SerializedProperty property)
		{
			string cacheKey = GetPropertyCacheKey(property);

			// Check cache
			if (_labelCache.TryGetValue(cacheKey, out GUIContent cachedLabel))
			{
				return cachedLabel;
			}

			LabelAttribute labelAttribute = GetAttribute<LabelAttribute>(property);
			string labelText = (labelAttribute == null) ? property.displayName : labelAttribute.Label;
			GUIContent label = new GUIContent(labelText);
			_labelCache[cacheKey] = label;
			return label;
		}

		public static void CallOnValueChangedCallbacks(SerializedProperty property)
		{
			OnValueChangedAttribute[] onValueChangedAttributes = GetAttributes<OnValueChangedAttribute>(property);
			if (onValueChangedAttributes.Length == 0)
			{
				return;
			}

			object target = GetTargetObjectWithProperty(property);
			property.serializedObject.ApplyModifiedProperties(); // We must apply modifications so that the new value is updated in the serialized object
			foreach (var onValueChangedAttribute in onValueChangedAttributes)
			{
				MethodInfo callbackMethod = ReflectionUtility.GetMethod(target, onValueChangedAttribute.CallbackName);
				if (callbackMethod != null && callbackMethod.ReturnType == typeof(void) && callbackMethod.GetParameters().Length == 0)
				{
					callbackMethod.Invoke(target, new object[] { });
				}
				else
				{
					string warning = string.Format("{0} can invoke only methods with 'void' return type and 0 parameters", onValueChangedAttribute.GetType().Name);
					Debug.LogWarning(warning, property.serializedObject.targetObject);
				}
			}
		}

		public static bool IsEnabled(SerializedProperty property)
		{
			ReadOnlyAttribute readOnlyAttribute = GetAttribute<ReadOnlyAttribute>(property);
			if (readOnlyAttribute != null)
			{
				return false;
			}

			EnableIfAttributeBase enableIfAttribute = GetAttribute<EnableIfAttributeBase>(property);
			if (enableIfAttribute == null)
			{
				return true;
			}

			object target = GetTargetObjectWithProperty(property);

			// deal with enum conditions
			if (enableIfAttribute.EnumValue != null)
			{
				Enum value = GetEnumValue(target, enableIfAttribute.Conditions[0]);
				if (value != null)
				{
					bool matched = value.GetType().GetCustomAttribute<FlagsAttribute>() == null ? enableIfAttribute.EnumValue.Equals(value) : value.HasFlag(enableIfAttribute.EnumValue);
					return matched != enableIfAttribute.Inverted;
				}

				string message = enableIfAttribute.GetType().Name + " needs a valid enum field, property or method name to work";
				Debug.LogWarning(message, property.serializedObject.targetObject);
				return false;
			}

			// deal with normal conditions
			List<bool> conditionValues = GetConditionValues(target, enableIfAttribute.Conditions);
			if (conditionValues.Count > 0)
			{
				bool enabled = GetConditionsFlag(conditionValues, enableIfAttribute.ConditionOperator, enableIfAttribute.Inverted);
				return enabled;
			}
			else
			{
				string message = enableIfAttribute.GetType().Name + " needs a valid boolean condition field, property or method name to work";
				Debug.LogWarning(message, property.serializedObject.targetObject);
				return false;
			}
		}

		public static bool IsVisible(SerializedProperty property)
		{
			ShowIfAttributeBase showIfAttribute = GetAttribute<ShowIfAttributeBase>(property);
			if (showIfAttribute == null)
			{
				return true;
			}

			object target = GetTargetObjectWithProperty(property);

			// deal with enum conditions
			if (showIfAttribute.EnumValue != null)
			{
				Enum value = GetEnumValue(target, showIfAttribute.Conditions[0]);
				if (value != null)
				{
					bool matched = value.GetType().GetCustomAttribute<FlagsAttribute>() == null ? showIfAttribute.EnumValue.Equals(value) : value.HasFlag(showIfAttribute.EnumValue);
					return matched != showIfAttribute.Inverted;
				}

				string message = showIfAttribute.GetType().Name + " needs a valid enum field, property or method name to work";
				Debug.LogWarning(message, property.serializedObject.targetObject);
				return false;
			}

			// deal with normal conditions
			List<bool> conditionValues = GetConditionValues(target, showIfAttribute.Conditions);
			if (conditionValues.Count > 0)
			{
				bool enabled = GetConditionsFlag(conditionValues, showIfAttribute.ConditionOperator, showIfAttribute.Inverted);
				return enabled;
			}
			else
			{
				string message = showIfAttribute.GetType().Name + " needs a valid boolean condition field, property or method name to work";
				Debug.LogWarning(message, property.serializedObject.targetObject);
				return false;
			}
		}

		/// <summary>
		///		Gets an enum value from reflection.
		/// </summary>
		/// <param name="target">The target object.</param>
		/// <param name="enumName">Name of a field, property, or method that returns an enum.</param>
		/// <returns>Null if can't find an enum value.</returns>
		internal static Enum GetEnumValue(object target, string enumName)
		{
			FieldInfo enumField = ReflectionUtility.GetField(target, enumName);
			if (enumField != null && enumField.FieldType.IsSubclassOf(typeof(Enum)))
			{
				return (Enum)enumField.GetValue(target);
			}

			PropertyInfo enumProperty = ReflectionUtility.GetProperty(target, enumName);
			if (enumProperty != null && enumProperty.PropertyType.IsSubclassOf(typeof(Enum)))
			{
				return (Enum)enumProperty.GetValue(target);
			}

			MethodInfo enumMethod = ReflectionUtility.GetMethod(target, enumName);
			if (enumMethod != null && enumMethod.ReturnType.IsSubclassOf(typeof(Enum)))
			{
				return (Enum)enumMethod.Invoke(target, null);
			}

			return null;
		}

		internal static List<bool> GetConditionValues(object target, string[] conditions)
		{
			List<bool> conditionValues = new List<bool>();
			foreach (var condition in conditions)
			{
				FieldInfo conditionField = ReflectionUtility.GetField(target, condition);
				if (conditionField != null && conditionField.FieldType == typeof(bool))
				{
					conditionValues.Add((bool)conditionField.GetValue(target));
				}

				PropertyInfo conditionProperty = ReflectionUtility.GetProperty(target, condition);
				if (conditionProperty != null && conditionProperty.PropertyType == typeof(bool))
				{
					conditionValues.Add((bool)conditionProperty.GetValue(target));
				}

				MethodInfo conditionMethod = ReflectionUtility.GetMethod(target, condition);
				if (conditionMethod != null && conditionMethod.ReturnType == typeof(bool) && conditionMethod.GetParameters().Length == 0)
				{
					conditionValues.Add((bool)conditionMethod.Invoke(target, null));
				}
			}

			return conditionValues;
		}

		internal static bool GetConditionsFlag(List<bool> conditionValues, EConditionOperator conditionOperator, bool invert)
		{
			bool flag;
			if (conditionOperator == EConditionOperator.And)
			{
				flag = true;
				foreach (var value in conditionValues)
				{
					flag = flag && value;
				}
			}
			else
			{
				flag = false;
				foreach (var value in conditionValues)
				{
					flag = flag || value;
				}
			}

			if (invert)
			{
				flag = !flag;
			}

			return flag;
		}

		public static Type GetPropertyType(SerializedProperty property)
		{
			object obj = GetTargetObjectOfProperty(property);
			Type objType = obj.GetType();
			return objType;
		}

		/// <summary>
		/// Gets the object the property represents.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetTargetObjectOfProperty(SerializedProperty property)
		{
			if (property == null)
			{
				return null;
			}

			string path = property.propertyPath.Replace(".Array.data[", "[");
			object obj = property.serializedObject.targetObject;
			string[] elements = path.Split('.');
			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					int bracketIndex = element.IndexOf("[");
					string elementName = element.Substring(0, bracketIndex);
					string indexStr = element.Substring(bracketIndex + 1, element.Length - bracketIndex - 2);
					int index = Convert.ToInt32(indexStr);
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		/// <summary>
		/// Gets the object that the property is a member of
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetTargetObjectWithProperty(SerializedProperty property)
		{
			string cacheKey = GetPropertyCacheKey(property);

			// Check cache
			if (_targetObjectCache.TryGetValue(cacheKey, out object cachedTarget))
			{
				return cachedTarget;
			}

			string path = property.propertyPath.Replace(".Array.data[", "[");
			object obj = property.serializedObject.targetObject;
			string[] elements = path.Split('.');
			for (int i = 0; i < elements.Length - 1; i++)
			{
				string element = elements[i];
				if (element.Contains("["))
				{
					int bracketIndex = element.IndexOf("[");
					string elementName = element.Substring(0, bracketIndex);
					string indexStr = element.Substring(bracketIndex + 1, element.Length - bracketIndex - 2);
					int index = Convert.ToInt32(indexStr);
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}

			_targetObjectCache[cacheKey] = obj;
			return obj;
		}

		private static object GetValue_Imp(object source, string name)
		{
			if (source == null)
			{
				return null;
			}

			Type type = source.GetType();
			while (type != null)
			{
				FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (field != null)
				{
					return field.GetValue(source);
				}

				PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (property != null)
				{
					return property.GetValue(source, null);
				}

				type = type.BaseType;
			}

			return null;
		}

		private static object GetValue_Imp(object source, string name, int index)
		{
			IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;
			if (enumerable == null)
			{
				return null;
			}

			IEnumerator enumerator = enumerable.GetEnumerator();
			for (int i = 0; i <= index; i++)
			{
				if (!enumerator.MoveNext())
				{
					return null;
				}
			}

			return enumerator.Current;
		}

		/// <summary>
		/// Clears all property utility caches. Call this when properties change or assemblies reload.
		/// </summary>
		public static void ClearCache()
		{
			_attributeCache.Clear();
			_fieldInfoCache.Clear();
			_targetObjectCache.Clear();
			_labelCache.Clear();
		}

	}


}