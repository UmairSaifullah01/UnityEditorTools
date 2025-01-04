using UnityEditor;
using UnityEngine;


namespace THEBADDEST.EditorTools
{


	[CustomPropertyDrawer(typeof(FoldoutHeaderAttribute))]
	public class FoldoutHeaderDrawer : PropertyDrawer
	{
		private static readonly GUIStyle FoldoutStyle = new GUIStyle(EditorStyles.foldout)
		{
			fontStyle = FontStyle.Bold
		};

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var attribute = (FoldoutHeaderAttribute)base.attribute;

			if (attribute.Expanded)
			{
				return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
			}
			else
			{
				return EditorGUIUtility.singleLineHeight;
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attribute = (FoldoutHeaderAttribute)base.attribute;

			// Create the foldout
			Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			attribute.Expanded = EditorGUI.Foldout(foldoutRect, attribute.Expanded, attribute.Header, true, FoldoutStyle);

			if (attribute.Expanded)
			{
				EditorGUI.indentLevel++;
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, property, label, true);
				EditorGUI.indentLevel--;
			}
		}
	}


}