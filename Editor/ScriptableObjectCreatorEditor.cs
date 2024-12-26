using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoScript))]
public class ScriptableObjectCreatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		MonoScript monoScript = (MonoScript)target;
		Type       scriptType = monoScript.GetClass();

		
		// Ensure the scriptType is not null, is a class, and is a subclass of ScriptableObject
		if (scriptType is { IsClass: true, Namespace: { } ns } && 
			scriptType.IsSubclassOf(typeof(ScriptableObject)) && 
			!ns.StartsWith("UnityEditor"))
		{ 
			if (GUILayout.Button("Create ScriptableObject Asset"))
			{
				CreateScriptableObjectAsset(scriptType);
			}
		}
	}

	private void CreateScriptableObjectAsset(Type scriptType)
	{
		// Get the script asset's file path
		string scriptPath = AssetDatabase.GetAssetPath(target);
		string directory  = Path.GetDirectoryName(scriptPath);
		string fileName   = scriptType.Name + ".asset";

		// Generate unique asset path
		string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, fileName));

		// Create the ScriptableObject instance
		ScriptableObject instance = ScriptableObject.CreateInstance(scriptType);
		if (instance == null)
		{
			Debug.LogError($"Failed to create an instance of {scriptType.Name}");
			return;
		}

		// Save the ScriptableObject as an asset
		AssetDatabase.CreateAsset(instance, assetPath);
		AssetDatabase.SaveAssets();

		// Highlight the newly created asset in the Project window
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = instance;
	}
}