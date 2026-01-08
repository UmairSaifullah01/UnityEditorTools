using UnityEngine;
using UnityEditor;

namespace THEBADDEST.EditorTools
{
    [CustomPropertyDrawer(typeof(SwitchAttribute))]
    public class SwitchPropertyDrawer : PropertyDrawerBase
    {
        private const float SwitchWidth = 50f;
        private const float SwitchHeight = 24f;
        private const float HandleSize = 20f;
        private const float HandlePadding = 2f;
        private const float TextSpacing = 8f;
        private const float ShadowOffset = 1f;

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                return GetPropertyHeight(property) + GetHelpBoxHeight();
            }

            return Mathf.Max(EditorGUIUtility.singleLineHeight, SwitchHeight);
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                string message = typeof(SwitchAttribute).Name + " can be used only on bool fields";
                DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
                EditorGUI.EndProperty();
                return;
            }

            SwitchAttribute switchAttribute = (SwitchAttribute)attribute;
            bool value = property.boolValue;

            // Calculate positions
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
            Rect switchRect = new Rect(rect.x + labelWidth, rect.y, SwitchWidth, SwitchHeight);
            Rect textRect = new Rect();

            if (switchAttribute.ShowText)
            {
                switchRect.width = SwitchWidth;
                textRect = new Rect(
                    switchRect.xMax + TextSpacing,
                    rect.y,
                    rect.width - labelWidth - SwitchWidth - TextSpacing,
                    EditorGUIUtility.singleLineHeight);
            }
            else
            {
                switchRect.width = Mathf.Min(SwitchWidth, rect.width - labelWidth);
            }

            // Center switch vertically
            float switchYOffset = (EditorGUIUtility.singleLineHeight - SwitchHeight) * 0.5f;
            switchRect.y += switchYOffset;

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw switch
            DrawSwitch(switchRect, value, switchAttribute);

            // Draw text
            if (switchAttribute.ShowText)
            {
                string text = value ? "True" : "False";
                // Grey text color to match the image
                Color textColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                GUI.color = textColor;
                EditorGUI.LabelField(textRect, text);
                GUI.color = Color.white;
            }

            // Handle click
            if (Event.current.type == EventType.MouseDown && switchRect.Contains(Event.current.mousePosition))
            {
                property.boolValue = !property.boolValue;
                Event.current.Use();
            }

            EditorGUI.EndProperty();
        }

        private void DrawSwitch(Rect rect, bool isOn, SwitchAttribute switchAttribute)
        {
            // Draw shadow
            Rect shadowRect = new Rect(rect.x + ShadowOffset, rect.y + ShadowOffset, rect.width, rect.height);
            EditorGUI.DrawRect(shadowRect, new Color(0, 0, 0, 0.2f));

            // Draw track (background)
            Color trackColor = isOn ? switchAttribute.OnColor : switchAttribute.OffColor;
            EditorGUI.DrawRect(rect, trackColor);

            // Calculate handle position
            float handleX = isOn 
                ? rect.xMax - HandleSize - HandlePadding 
                : rect.x + HandlePadding;
            float handleY = rect.y + (rect.height - HandleSize) * 0.5f;

            Rect handleRect = new Rect(handleX, handleY, HandleSize, HandleSize);

            // Draw handle shadow
            Rect handleShadowRect = new Rect(
                handleRect.x + ShadowOffset * 0.5f,
                handleRect.y + ShadowOffset * 0.5f,
                HandleSize,
                HandleSize);
            EditorGUI.DrawRect(handleShadowRect, new Color(0, 0, 0, 0.3f));

            // Draw handle
            // When ON: black handle, when OFF: light purple/white handle
            Color handleColor = isOn 
                ? new Color(0.15f, 0.15f, 0.15f) // Black when on
                : new Color(0.85f, 0.75f, 0.95f); // Light purple when off
            EditorGUI.DrawRect(handleRect, handleColor);

            // Draw handle border for depth
            Color borderColor = new Color(0, 0, 0, 0.1f);
            DrawRoundedRect(handleRect, borderColor, 1f);
        }

        private void DrawRoundedRect(Rect rect, Color color, float borderWidth)
        {
            // Simple border effect
            Rect topBorder = new Rect(rect.x, rect.y, rect.width, borderWidth);
            Rect bottomBorder = new Rect(rect.x, rect.yMax - borderWidth, rect.width, borderWidth);
            Rect leftBorder = new Rect(rect.x, rect.y, borderWidth, rect.height);
            Rect rightBorder = new Rect(rect.xMax - borderWidth, rect.y, borderWidth, rect.height);

            EditorGUI.DrawRect(topBorder, color);
            EditorGUI.DrawRect(bottomBorder, color);
            EditorGUI.DrawRect(leftBorder, color);
            EditorGUI.DrawRect(rightBorder, color);
        }
    }
}

