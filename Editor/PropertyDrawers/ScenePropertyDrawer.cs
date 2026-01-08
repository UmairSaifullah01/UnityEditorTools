using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace THEBADDEST.EditorTools
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class ScenePropertyDrawer : PropertyDrawerBase
    {
        private const string SceneListItem = "{0} ({1})";
        private const string ScenePattern = @".+\/(.+)\.unity";
        private const string TypeWarningMessage = "{0} must be an int or a string";
        private const string BuildSettingsWarningMessage = "No scenes in the build settings";

        // Cache for scene lists to avoid repeated queries
        private static string[] _cachedScenes;
        private static string[] _cachedSceneOptions;
        private static int _lastBuildSettingsHash;

        /// <summary>
        /// Invalidates the scene cache. Called when build settings change or assemblies reload.
        /// </summary>
        public static void InvalidateSceneCache()
        {
            _cachedScenes = null;
            _cachedSceneOptions = null;
            _lastBuildSettingsHash = 0;
        }

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            bool validPropertyType = property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Integer;
            bool anySceneInBuildSettings = GetScenes().Length > 0;

            return (validPropertyType && anySceneInBuildSettings)
                ? GetPropertyHeight(property)
                : GetPropertyHeight(property) + GetHelpBoxHeight();
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            string[] scenes = GetScenes();
            bool anySceneInBuildSettings = scenes.Length > 0;
            if (!anySceneInBuildSettings)
            {
                DrawDefaultPropertyAndHelpBox(rect, property, BuildSettingsWarningMessage, MessageType.Warning);
                return;
            }

            string[] sceneOptions = GetSceneOptions(scenes);
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(rect, property, label, scenes, sceneOptions);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(rect, property, label, sceneOptions);
                    break;
                default:
                    string message = string.Format(TypeWarningMessage, property.name);
                    DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private string[] GetScenes()
        {
            // Check if build settings have changed
            int currentHash = EditorBuildSettings.scenes.GetHashCode();
            if (_cachedScenes == null || _lastBuildSettingsHash != currentHash)
            {
                // Rebuild cache
                _lastBuildSettingsHash = currentHash;
                
                // Use List to avoid multiple allocations
                System.Collections.Generic.List<string> sceneList = new System.Collections.Generic.List<string>();
                EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
                
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    if (buildScenes[i].enabled)
                    {
                        Match match = Regex.Match(buildScenes[i].path, ScenePattern);
                        if (match.Success)
                        {
                            sceneList.Add(match.Groups[1].Value);
                        }
                    }
                }
                
                _cachedScenes = sceneList.ToArray();
                
                // Rebuild scene options cache
                _cachedSceneOptions = new string[_cachedScenes.Length];
                for (int i = 0; i < _cachedScenes.Length; i++)
                {
                    _cachedSceneOptions[i] = string.Format(SceneListItem, _cachedScenes[i], i);
                }
            }
            
            return _cachedScenes;
        }

        private string[] GetSceneOptions(string[] scenes)
        {
            // Ensure cache is built
            GetScenes();
            return _cachedSceneOptions;
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] scenes, string[] sceneOptions)
        {
            int index = IndexOf(scenes, property.stringValue);
            int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
            string newScene = scenes[newIndex];

            if (!property.stringValue.Equals(newScene, StringComparison.Ordinal))
            {
                property.stringValue = scenes[newIndex];
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] sceneOptions)
        {
            int index = property.intValue;
            int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);

            if (property.intValue != newIndex)
            {
                property.intValue = newIndex;
            }
        }

        private static int IndexOf(string[] scenes, string scene)
        {
            var index = Array.IndexOf(scenes, scene);
            return Mathf.Clamp(index, 0, scenes.Length - 1);
        }
    }
}
