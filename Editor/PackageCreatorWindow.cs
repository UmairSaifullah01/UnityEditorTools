using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public class PackageCreatorWindow : EditorWindow
{
    private string packageName = "MyPackage";
    private bool includeEditorFolder = true;
    private bool includeRuntimeFolder = true;
    private bool includeTestsFolder = true;
    private bool includeDocumentationFolder = true;
    private bool includeChangeLog = true;
    private bool includeReadMe = true;

    private string unityVersion = "2020.3";
    private string documentationUrl = "";
    private string repositoryUrl = "";
    private string repositoryRevision = "";
    private string customPath = "Assets";

    [MenuItem("Tools/Package Creator")]
    public static void ShowWindow()
    {
        GetWindow<PackageCreatorWindow>("Package Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Package Creator", EditorStyles.boldLabel);

        packageName = EditorGUILayout.TextField("Package Name", packageName);
        unityVersion = EditorGUILayout.TextField("Unity Version", unityVersion);

        GUILayout.Label("Include Folders:", EditorStyles.label);
        includeEditorFolder = EditorGUILayout.ToggleLeft("Editor", includeEditorFolder);
        includeRuntimeFolder = EditorGUILayout.ToggleLeft("Runtime", includeRuntimeFolder);
        includeTestsFolder = EditorGUILayout.ToggleLeft("Tests", includeTestsFolder);
        includeDocumentationFolder = EditorGUILayout.ToggleLeft("Documentation~", includeDocumentationFolder);

        GUILayout.Label("Include Files:", EditorStyles.label);
        includeChangeLog = EditorGUILayout.ToggleLeft("CHANGELOG.md", includeChangeLog);
        includeReadMe = EditorGUILayout.ToggleLeft("README.md", includeReadMe);

        GUILayout.Label("Additional Information:", EditorStyles.label);
        documentationUrl = EditorGUILayout.TextField("Documentation URL", documentationUrl);
        repositoryUrl = EditorGUILayout.TextField("Repository URL", repositoryUrl);
        repositoryRevision = EditorGUILayout.TextField("Repository Revision", repositoryRevision);
        customPath = EditorGUILayout.TextField("Save Path", customPath);

        GUILayout.Label("Folder Preview:", EditorStyles.label);
        if (includeEditorFolder) GUILayout.Label("- Editor");
        if (includeRuntimeFolder) GUILayout.Label("- Runtime");
        if (includeTestsFolder) GUILayout.Label("- Tests");
        if (includeDocumentationFolder) GUILayout.Label("- Documentation~");
        if (includeChangeLog) GUILayout.Label("- CHANGELOG.md");
        if (includeReadMe) GUILayout.Label("- README.md");

        if (GUILayout.Button("Create Package Structure"))
        {
            CreatePackageStructure();
        }

        if (GUILayout.Button("Create Git Repository"))
        {
            CreateGitRepository();
        }
    }

    private void CreatePackageStructure()
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            Debug.LogError("Package name cannot be empty!");
            return;
        }

        string packagePath = Path.Combine(customPath, packageName);
        if (Directory.Exists(packagePath))
        {
            Debug.LogWarning("A folder with this package name already exists. Please choose a different name.");
            return;
        }

        try
        {
            Directory.CreateDirectory(packagePath);

            if (includeEditorFolder)
                Directory.CreateDirectory(Path.Combine(packagePath, "Editor"));

            if (includeRuntimeFolder)
                Directory.CreateDirectory(Path.Combine(packagePath, "Runtime"));

            if (includeTestsFolder)
                Directory.CreateDirectory(Path.Combine(packagePath, "Tests"));

            if (includeDocumentationFolder)
                Directory.CreateDirectory(Path.Combine(packagePath, "Documentation~"));

            string packageJsonPath = Path.Combine(packagePath, "package.json");
            File.WriteAllText(packageJsonPath, GeneratePackageJson());

            if (includeChangeLog)
            {
                string changelogPath = Path.Combine(packagePath, "CHANGELOG.md");
                File.WriteAllText(changelogPath, "# Changelog\n\nAll notable changes to this project will be documented in this file.");
            }

            if (includeReadMe)
            {
                string readmePath = Path.Combine(packagePath, "README.md");
                File.WriteAllText(readmePath, $"# {packageName}\n\nA description of your Unity package.");
            }

            AssetDatabase.Refresh();
            Debug.Log("Package structure created successfully at: " + packagePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to create package structure: " + ex.Message);
        }
    }

    private string GeneratePackageJson()
    {
        return $@"{{
	""name"": ""com.thebaddest.{packageName.ToLower()}"",
	""version"": ""1.0.0"",
	""displayName"": ""{packageName}"",
	""description"": ""A short description of your package."",
	""unity"": ""{unityVersion}"",
	""author"": {{
		""name"": ""Umair Saifullah"",
		""email"": ""contact@umairsaifullah.com"",
		""url"": ""https://www.umairsaifullah.com""
	}},
	""documentationUrl"": ""{documentationUrl}"",
	""repository"": {{
		""url"": ""{repositoryUrl}"",
		""type"": ""git"",
		""revision"": ""{repositoryRevision}""
	}},
	""dependencies"": {{}}
}}
			";
    }

    private void CreateGitRepository()
    {
        string packagePath = Path.Combine(customPath, packageName);

        if (!Directory.Exists(packagePath))
        {
            Debug.LogError("Package folder does not exist. Create the package structure first.");
            return;
        }

        try
        {
            RunCommand("git", "init", packagePath);
            RunCommand("git", "checkout -b main", packagePath);
            RunCommand("git", "checkout -b develop", packagePath);

            Debug.Log("Git repository initialized with 'main' and 'develop' branches at: " + packagePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to create Git repository: " + ex.Message);
        }
    }

    private void RunCommand(string command, string arguments, string workingDirectory)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(processInfo))
        {
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
                Debug.Log(output);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);
        }
    }
}
