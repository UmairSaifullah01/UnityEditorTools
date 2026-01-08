using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.EditorTools
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class UInspector : UnityEditor.Editor
    {
        private List<SerializedProperty> _serializedProperties = new List<SerializedProperty>();
        private List<FieldInfo> _nonSerializedFields = new List<FieldInfo>();
        private List<PropertyInfo> _nativeProperties = new List<PropertyInfo>();
        private List<MethodInfo> _methods = new List<MethodInfo>();
        private Dictionary<string, SavedBool> _foldouts = new Dictionary<string, SavedBool>();
        private static GUIStyle _headerStyle;

        protected virtual void OnEnable()
        {
            _nonSerializedFields.Clear();
            foreach (var field in ReflectionUtility.GetAllFields(
                target, f => f.GetCustomAttributes(typeof(ShowNonSerializedFieldAttribute), true).Length > 0))
            {
                _nonSerializedFields.Add(field);
            }

            _nativeProperties.Clear();
            foreach (var property in ReflectionUtility.GetAllProperties(
                target, p => p.GetCustomAttributes(typeof(ShowNativePropertyAttribute), true).Length > 0))
            {
                _nativeProperties.Add(property);
            }

            _methods.Clear();
            foreach (var method in ReflectionUtility.GetAllMethods(
                target, m => m.GetCustomAttributes(typeof(THEBADDEST.ButtonAttribute), true).Length > 0))
            {
                _methods.Add(method);
            }
        }

        protected virtual void OnDisable()
        {
            ReorderableListPropertyDrawer.Instance.ClearCache();
            PropertyUtility.ClearCache();
            ReflectionUtility.ClearCache();
        }

        public override void OnInspectorGUI()
        {
            GetSerializedProperties(ref _serializedProperties);

            // Check for any Naughty attributes without LINQ
            bool anyNaughtyAttribute = false;
            for (int i = 0; i < _serializedProperties.Count; i++)
            {
                if (PropertyUtility.GetAttribute<IUAttribute>(_serializedProperties[i]) != null)
                {
                    anyNaughtyAttribute = true;
                    break;
                }
            }

            if (!anyNaughtyAttribute)
            {
                DrawDefaultInspector();
            }
            else
            {
                DrawSerializedProperties();
            }

            DrawNonSerializedFields();
            DrawNativeProperties();
            DrawButtons();
        }

        protected void GetSerializedProperties(ref List<SerializedProperty> outSerializedProperties)
        {
            outSerializedProperties.Clear();
            using (var iterator = serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        outSerializedProperties.Add(serializedObject.FindProperty(iterator.name));
                    }
                    while (iterator.NextVisible(false));
                }
            }
        }

        protected void DrawSerializedProperties()
        {
            serializedObject.Update();

            // Draw non-grouped serialized properties
            for (int i = 0; i < _serializedProperties.Count; i++)
            {
                SerializedProperty property = _serializedProperties[i];
                if (PropertyUtility.GetAttribute<IGroupAttribute>(property) == null)
                {
                    if (property.name.Equals("m_Script", System.StringComparison.Ordinal))
                    {
                        using (new EditorGUI.DisabledScope(disabled: true))
                        {
                            EditorGUILayout.PropertyField(property);
                        }
                    }
                    else
                    {
                        NaughtyEditorGUI.PropertyField_Layout(property, includeChildren: true);
                    }
                }
            }

            // Draw grouped serialized properties
            Dictionary<string, List<SerializedProperty>> groupedProperties = GetGroupedProperties(_serializedProperties);
            foreach (var group in groupedProperties)
            {
                // Filter visible properties
                List<SerializedProperty> visibleProperties = new List<SerializedProperty>();
                for (int i = 0; i < group.Value.Count; i++)
                {
                    if (PropertyUtility.IsVisible(group.Value[i]))
                    {
                        visibleProperties.Add(group.Value[i]);
                    }
                }

                if (visibleProperties.Count == 0)
                {
                    continue;
                }

                NaughtyEditorGUI.BeginBoxGroup_Layout(group.Key);
                for (int i = 0; i < visibleProperties.Count; i++)
                {
                    NaughtyEditorGUI.PropertyField_Layout(visibleProperties[i], includeChildren: true);
                }
                NaughtyEditorGUI.EndBoxGroup_Layout();
            }

            // Draw foldout serialized properties
            Dictionary<string, List<SerializedProperty>> foldoutProperties = GetFoldoutProperties(_serializedProperties);
            foreach (var group in foldoutProperties)
            {
                // Filter visible properties
                List<SerializedProperty> visibleProperties = new List<SerializedProperty>();
                for (int i = 0; i < group.Value.Count; i++)
                {
                    if (PropertyUtility.IsVisible(group.Value[i]))
                    {
                        visibleProperties.Add(group.Value[i]);
                    }
                }

                if (visibleProperties.Count == 0)
                {
                    continue;
                }

                if (!_foldouts.ContainsKey(group.Key))
                {
                    _foldouts[group.Key] = new SavedBool($"{target.GetInstanceID()}.{group.Key}", false);
                }

                _foldouts[group.Key].Value = EditorGUILayout.Foldout(_foldouts[group.Key].Value, group.Key, true);
                if (_foldouts[group.Key].Value)
                {
                    for (int i = 0; i < visibleProperties.Count; i++)
                    {
                        NaughtyEditorGUI.PropertyField_Layout(visibleProperties[i], true);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawNonSerializedFields(bool drawHeader = false)
        {
            if (_nonSerializedFields.Count > 0)
            {
                if (drawHeader)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Non-Serialized Fields", GetHeaderGUIStyle());
                    NaughtyEditorGUI.HorizontalLine(
                        EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
                }

                for (int i = 0; i < _nonSerializedFields.Count; i++)
                {
                    NaughtyEditorGUI.NonSerializedField_Layout(serializedObject.targetObject, _nonSerializedFields[i]);
                }
            }
        }

        protected void DrawNativeProperties(bool drawHeader = false)
        {
            if (_nativeProperties.Count > 0)
            {
                if (drawHeader)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Native Properties", GetHeaderGUIStyle());
                    NaughtyEditorGUI.HorizontalLine(
                        EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
                }

                for (int i = 0; i < _nativeProperties.Count; i++)
                {
                    NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, _nativeProperties[i]);
                }
            }
        }

        protected void DrawButtons(bool drawHeader = false)
        {
            if (_methods.Count > 0)
            {
                if (drawHeader)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Buttons", GetHeaderGUIStyle());
                    NaughtyEditorGUI.HorizontalLine(
                        EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
                }

                for (int i = 0; i < _methods.Count; i++)
                {
                    NaughtyEditorGUI.Button(serializedObject.targetObject, _methods[i]);
                }
            }
        }

        private static Dictionary<string, List<SerializedProperty>> GetGroupedProperties(List<SerializedProperty> properties)
        {
            Dictionary<string, List<SerializedProperty>> grouped = new Dictionary<string, List<SerializedProperty>>();
            
            for (int i = 0; i < properties.Count; i++)
            {
                SerializedProperty property = properties[i];
                BoxGroupAttribute boxGroup = PropertyUtility.GetAttribute<BoxGroupAttribute>(property);
                if (boxGroup != null)
                {
                    if (!grouped.TryGetValue(boxGroup.Name, out List<SerializedProperty> groupList))
                    {
                        groupList = new List<SerializedProperty>();
                        grouped[boxGroup.Name] = groupList;
                    }
                    groupList.Add(property);
                }
            }

            return grouped;
        }

        private static Dictionary<string, List<SerializedProperty>> GetFoldoutProperties(List<SerializedProperty> properties)
        {
            Dictionary<string, List<SerializedProperty>> foldoutGroups = new Dictionary<string, List<SerializedProperty>>();
            
            for (int i = 0; i < properties.Count; i++)
            {
                SerializedProperty property = properties[i];
                FoldoutAttribute foldout = PropertyUtility.GetAttribute<FoldoutAttribute>(property);
                if (foldout != null)
                {
                    if (!foldoutGroups.TryGetValue(foldout.Name, out List<SerializedProperty> groupList))
                    {
                        groupList = new List<SerializedProperty>();
                        foldoutGroups[foldout.Name] = groupList;
                    }
                    groupList.Add(property);
                }
            }

            return foldoutGroups;
        }

        private static GUIStyle GetHeaderGUIStyle()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                _headerStyle.fontStyle = FontStyle.Bold;
                _headerStyle.alignment = TextAnchor.UpperCenter;
            }

            return _headerStyle;
        }
    }
}
