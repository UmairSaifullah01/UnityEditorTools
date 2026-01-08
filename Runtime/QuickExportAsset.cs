#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace THEBADDEST
{
	/// <summary>
	/// Represents a catalog of asset types that can be organized into folders.
	/// </summary>
	[System.Serializable]
	public class Catalog
	{
		public string catalogName;
		public string path;
		public string[] types;
	}

	/// <summary>
	/// Tracks an object that has been moved from one path to another.
	/// </summary>
	[System.Serializable]
	public class MovedObject
	{
		public Object root;
		public string oldPath;
		public string newPath;
	}

	/// <summary>
	/// Editor window for quickly organizing and exporting Unity assets into a structured package.
	/// </summary>
	public class QuickExportAsset : EditorWindow
	{
		private const string DEFAULT_FOLDER_NAME = "Game Art";
		private const string ASSETS_ROOT = "Assets";
		private const string SCRIPTS_CATALOG_NAME = "Scripts";
		private const string OTHER_CATALOG_NAME = "Other";

		[SerializeField] private string packageName = DEFAULT_FOLDER_NAME;
		[SerializeField] private Object[] dataObjects;
		[SerializeField] private bool moveScripts = true;

		private readonly Catalog[] catalogs = new[]
		{
			// Scenes
			new Catalog { catalogName = "Scenes", types = new[] { ".unity", ".lighting" } },
			
			// Textures & Images
			new Catalog { catalogName = "Textures", types = new[] { ".png", ".tga", ".jpg", ".jpeg", ".exr", ".hdr", ".tiff", ".tif", ".psd", ".gif", ".bmp", ".dds" } },
			
			// 3D Models
			new Catalog { catalogName = "Models", types = new[] { ".fbx", ".obj", ".dae", ".3ds", ".dxf", ".blend", ".max", ".ma", ".mb", ".x", ".ase" } },
			
			// Materials & Shaders
			new Catalog { catalogName = "Materials", types = new[] { ".mat" } },
			new Catalog { catalogName = "Shaders", types = new[] { ".shader", ".cginc", ".hlsl", ".compute", ".shadergraph", ".shadersubgraph" } },
			
			// Prefabs & GameObjects
			new Catalog { catalogName = "Prefabs", types = new[] { ".prefab", ".prefabvariant" } },
			
			// Scripts
			new Catalog { catalogName = "Scripts", types = new[] { ".cs", ".js", ".boo" } },
			
			// Animations
			new Catalog { catalogName = "Animations", types = new[] { ".anim", ".controller", ".overrideController", ".mask" } },
			
			// Audio
			new Catalog { catalogName = "Audio", types = new[] { ".mp3", ".wav", ".ogg", ".aiff", ".mod", ".it", ".s3m", ".xm", ".aac" } },
			
			// Video
			new Catalog { catalogName = "Video", types = new[] { ".mp4", ".mov", ".avi", ".asf", ".webm" } },
			
			// Fonts
			new Catalog { catalogName = "Fonts", types = new[] { ".ttf", ".otf", ".dfont", ".fon" } },
			
			// Physics
			new Catalog { catalogName = "Physics", types = new[] { ".physicsMaterial", ".physicsMaterial2D" } },
			
			// Text & Data
			new Catalog { catalogName = "Text", types = new[] { ".txt", ".json", ".xml", ".csv", ".bytes", ".yaml", ".yml" } },
			
			// Presets
			new Catalog { catalogName = "Presets", types = new[] { ".preset" } },
			
			// Timeline
			new Catalog { catalogName = "Timeline", types = new[] { ".playable", ".signal" } },
			
			// Assembly Definitions
			new Catalog { catalogName = "Assemblies", types = new[] { ".asmdef", ".asmref" } },
			
			// Packages
			new Catalog { catalogName = "Packages", types = new[] { ".tgz", ".unitypackage" } },
			
			// Terrain
			new Catalog { catalogName = "Terrain", types = new[] { ".asset" } }, // Terrain data assets
			
			// Sprites & Atlases
			new Catalog { catalogName = "Sprites", types = new[] { ".spriteatlas", ".spriteatlasv2" } },
			
			// Render Textures
			new Catalog { catalogName = "RenderTextures", types = new[] { ".renderTexture", ".cubemap" } },
			
			// Lighting
			new Catalog { catalogName = "Lighting", types = new[] { ".lighting", ".lightmap", ".exr" } },
			
			// UI
			new Catalog { catalogName = "UI", types = new[] { ".sprite", ".guiskin", ".fontsettings" } },
			
			// NavMesh
			new Catalog { catalogName = "NavMesh", types = new[] { ".navmesh", ".navmeshData" } },
			
			// Build Settings
			new Catalog { catalogName = "BuildSettings", types = new[] { ".asset" } }, // Build settings assets
			
			// Other (catch-all for undefined types - must be last)
			new Catalog { catalogName = OTHER_CATALOG_NAME, types = null } // null means it catches everything else
		};

		private bool folderCreated;
		private List<MovedObject> movedObjects = new List<MovedObject>();
		private SerializedProperty dataObjectsProperty;
		private SerializedObject serializedObject;
		private string mainFolder;

		[MenuItem("Window/THEBADDEST/Quick Export Asset")]
		private static void Init()
		{
			QuickExportAsset window = GetWindow<QuickExportAsset>("Quick Export Asset");
			window.Show();
		}

		private void OnEnable()
		{
			serializedObject = new SerializedObject(this);
			dataObjectsProperty = serializedObject.FindProperty("dataObjects");
		}

		private void OnGUI()
		{
			if (serializedObject == null)
			{
				OnEnable();
			}

			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Developed By Umair Saifullah", MessageType.Info, true);
			EditorGUILayout.Space();

			// Package name input
			packageName = EditorGUILayout.TextField("Package Name", packageName);
			if (string.IsNullOrEmpty(packageName))
			{
				EditorGUILayout.HelpBox("Package name cannot be empty.", MessageType.Warning);
			}

			EditorGUILayout.Space();

			// Create folders button
			CreateFolders();

			// Data fields (only shown after folders are created)
			if (folderCreated)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Asset Organization", EditorStyles.boldLabel);
				EditorGUILayout.Space();

				serializedObject.Update();
				EditorGUILayout.PropertyField(dataObjectsProperty, new GUIContent("Additional Objects"), true);
				moveScripts = EditorGUILayout.Toggle("Move Scripts", moveScripts);
				serializedObject.ApplyModifiedProperties();

				EditorGUILayout.Space();

				// Check dependencies button
				CheckDependencies();

				EditorGUILayout.Space();

				// Organize and export section
				if (HasObjectsToOrganize())
				{
					EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
					EditorGUILayout.Space();

					OrganiseObjects();
					EditorGUILayout.Space();
					ExportFolders();
					EditorGUILayout.Space();
					Revert();
				}
				else
				{
					EditorGUILayout.HelpBox("Select objects in the Project window or add them to 'Additional Objects' to organize.", MessageType.Info);
				}
			}
		}

		private bool HasObjectsToOrganize()
		{
			return (dataObjects != null && dataObjects.Length > 0) || (Selection.objects != null && Selection.objects.Length > 0);
		}

		private void CreateFolders()
		{
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(packageName));
			if (GUILayout.Button("Create Folders", GUILayout.Height(30)))
			{
				if (CreateFolderStructure())
				{
					folderCreated = true;
					AssetDatabase.Refresh();
					Debug.Log($"Folder structure created successfully at: {mainFolder}");
				}
			}
			EditorGUI.EndDisabledGroup();
		}

		private bool CreateFolderStructure()
		{
			try
			{
				mainFolder = $"{ASSETS_ROOT}/{packageName}";

				// Create main folder structure (supports nested paths)
				string[] directories = packageName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string currentPath = ASSETS_ROOT;

				foreach (string directory in directories)
				{
					if (string.IsNullOrEmpty(directory))
						continue;

					string folderPath = Path.Combine(currentPath, directory);
					if (!AssetDatabase.IsValidFolder(folderPath))
					{
						string guid = AssetDatabase.CreateFolder(currentPath, directory);
						if (string.IsNullOrEmpty(guid))
						{
							Debug.LogError($"Failed to create folder: {folderPath}");
							return false;
						}
						folderPath = AssetDatabase.GUIDToAssetPath(guid);
					}

					currentPath = folderPath;
				}

				mainFolder = currentPath;

				// Create catalog folders
				foreach (Catalog catalog in catalogs)
				{
					catalog.path = $"{mainFolder}/{catalog.catalogName}";
					if (!AssetDatabase.IsValidFolder(catalog.path))
					{
						string guid = AssetDatabase.CreateFolder(mainFolder, catalog.catalogName);
						if (string.IsNullOrEmpty(guid))
						{
							Debug.LogWarning($"Failed to create catalog folder: {catalog.path}");
							continue;
						}
						catalog.path = AssetDatabase.GUIDToAssetPath(guid);
					}
				}

				return true;
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Error creating folder structure: {e.Message}");
				return false;
			}
		}

		private void CheckDependencies()
		{
			if (dataObjects == null || dataObjects.Length == 0)
				return;

			EditorGUI.BeginDisabledGroup(dataObjects.Any(obj => obj == null));
			if (GUILayout.Button("Check Dependencies"))
			{
				Object[] dependencies = EditorUtility.CollectDependencies(dataObjects);
				Selection.objects = dependencies;
				Debug.Log($"Found {dependencies.Length} dependencies.");
			}
			EditorGUI.EndDisabledGroup();
		}

		private void OrganiseObjects()
		{
			EditorGUI.BeginDisabledGroup(!HasObjectsToOrganize());
			if (GUILayout.Button("Organise", GUILayout.Height(30)))
			{
				OrganizeSelectedObjects();
			}
			EditorGUI.EndDisabledGroup();
		}

		private void OrganizeSelectedObjects()
		{
			movedObjects.Clear();
			List<string> errors = new List<string>();

			// Organize selected objects
			if (Selection.objects != null && Selection.objects.Length > 0)
			{
				foreach (Object obj in Selection.objects)
				{
					if (obj == null)
						continue;

					if (TryMoveObject(obj, out MovedObject movedObject, out string error))
					{
						movedObjects.Add(movedObject);
					}
					else if (!string.IsNullOrEmpty(error))
					{
						errors.Add(error);
					}
				}
			}

			// Organize data objects
			if (dataObjects != null && dataObjects.Length > 0)
			{
				foreach (Object obj in dataObjects)
				{
					if (obj == null)
						continue;

					if (TryMoveObject(obj, out MovedObject movedObject, out string error))
					{
						movedObjects.Add(movedObject);
					}
					else if (!string.IsNullOrEmpty(error))
					{
						errors.Add(error);
					}
				}
			}

			AssetDatabase.Refresh();

			if (errors.Count > 0)
			{
				Debug.LogWarning($"Organized {movedObjects.Count} objects. {errors.Count} errors occurred:\n{string.Join("\n", errors)}");
			}
			else
			{
				Debug.Log($"Successfully organized {movedObjects.Count} objects!");
			}
		}

		private bool TryMoveObject(Object obj, out MovedObject movedObject, out string error)
		{
			movedObject = new MovedObject { root = obj };
			error = null;

			string oldPath = AssetDatabase.GetAssetPath(obj);
			if (string.IsNullOrEmpty(oldPath))
			{
				error = $"No asset path found for {obj.name}";
				return false;
			}

			if (!IsValidPath(oldPath))
			{
				movedObject.oldPath = oldPath;
				movedObject.newPath = oldPath;
				return false; // Not an error, just skipped
			}

			movedObject.oldPath = oldPath;
			string fileName = Path.GetFileName(oldPath);
			Catalog catalog = GetCatalogForFile(fileName);

			// Use catalog path if found, otherwise fall back to main folder
			string newFolderPath = catalog != null && !string.IsNullOrEmpty(catalog.path) ? catalog.path : mainFolder;
			movedObject.newPath = $"{newFolderPath}/{fileName}";

			// Check if target path already exists
			if (AssetDatabase.LoadAssetAtPath<Object>(movedObject.newPath) != null)
			{
				error = $"Target path already exists: {movedObject.newPath}";
				return false;
			}

			string moveResult = AssetDatabase.MoveAsset(oldPath, movedObject.newPath);
			if (!string.IsNullOrEmpty(moveResult))
			{
				error = $"Failed to move {oldPath}: {moveResult}";
				return false;
			}

			return true;
		}

		private bool IsValidPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			// Don't move assets from special folders
			if (path.Contains("Plugins") || path.Contains("Resources") || path.Contains("Editor"))
				return false;

			// Don't move assets that are already in the target folder
			if (path.Contains(packageName))
				return false;

			// Check if scripts should be moved
			if (!moveScripts)
			{
				Catalog catalog = GetCatalogForFile(Path.GetFileName(path));
				if (catalog != null && catalog.catalogName == SCRIPTS_CATALOG_NAME)
					return false;
			}

			return true;
		}

		private Catalog GetCatalogForFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return GetOtherCatalog();

			fileName = fileName.ToLower();
			
			// Check all catalogs except "Other"
			foreach (Catalog catalog in catalogs)
			{
				// Skip the "Other" catalog during matching
				if (catalog.catalogName == OTHER_CATALOG_NAME || catalog.types == null)
					continue;

				foreach (string type in catalog.types)
				{
					if (fileName.EndsWith(type.ToLower()))
					{
						return catalog;
					}
				}
			}

			// If no match found, return "Other" catalog
			return GetOtherCatalog();
		}

		private Catalog GetOtherCatalog()
		{
			foreach (Catalog catalog in catalogs)
			{
				if (catalog.catalogName == OTHER_CATALOG_NAME)
					return catalog;
			}
			return null;
		}

		private void ExportFolders()
		{
			if (string.IsNullOrEmpty(mainFolder) || !AssetDatabase.IsValidFolder(mainFolder))
			{
				EditorGUILayout.HelpBox("Main folder is not valid. Please create folders first.", MessageType.Warning);
				return;
			}

			EditorGUI.BeginDisabledGroup(movedObjects.Count == 0);
			if (GUILayout.Button("Export Package", GUILayout.Height(30)))
			{
				string packagePath = EditorUtility.SaveFilePanel("Export Unity Package", "", packageName, "unitypackage");
				if (!string.IsNullOrEmpty(packagePath))
				{
					try
					{
						AssetDatabase.ExportPackage(
							mainFolder,
							packagePath,
							ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
						);

						Debug.Log($"Package exported successfully to: {packagePath}");
						EditorUtility.RevealInFinder(packagePath);
					}
					catch (System.Exception e)
					{
						Debug.LogError($"Failed to export package: {e.Message}");
					}
				}
			}
			EditorGUI.EndDisabledGroup();
		}

		private void Revert()
		{
			EditorGUI.BeginDisabledGroup(movedObjects == null || movedObjects.Count == 0);
			if (GUILayout.Button("Revert Changes", GUILayout.Height(30)))
			{
				RevertMoves();
			}
			EditorGUI.EndDisabledGroup();
		}

		private void RevertMoves()
		{
			if (movedObjects == null || movedObjects.Count == 0)
			{
				Debug.LogWarning("No moves to revert.");
				return;
			}

			List<string> errors = new List<string>();
			int revertedCount = 0;

			foreach (MovedObject movedObject in movedObjects)
			{
				if (movedObject == null || string.IsNullOrEmpty(movedObject.newPath) || string.IsNullOrEmpty(movedObject.oldPath))
					continue;

				// Only revert if the file is still at the new path
				if (AssetDatabase.LoadAssetAtPath<Object>(movedObject.newPath) != null)
				{
					string revertResult = AssetDatabase.MoveAsset(movedObject.newPath, movedObject.oldPath);
					if (string.IsNullOrEmpty(revertResult))
					{
						revertedCount++;
					}
					else
					{
						errors.Add($"Failed to revert {movedObject.newPath}: {revertResult}");
					}
				}
			}

			movedObjects.Clear();
			AssetDatabase.Refresh();

			if (errors.Count > 0)
			{
				Debug.LogWarning($"Reverted {revertedCount} objects. {errors.Count} errors occurred:\n{string.Join("\n", errors)}");
			}
			else
			{
				Debug.Log($"Successfully reverted {revertedCount} objects!");
			}
		}
	}
}
#endif
