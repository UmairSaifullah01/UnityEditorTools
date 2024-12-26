using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GPTDocumentationWindow : EditorWindow
{
    private string selectedFolderPath = "";
    private List<string> scriptPaths = new List<string>();
    private StringBuilder markdownBuilder = new StringBuilder();
    private int currentScriptIndex = 0;
    private bool summariesGenerated = false;

    private static string apiKey = ""; // Replace with your GPT key

    [MenuItem("Tools/GPT Documentation Generator")]
    public static void ShowWindow()
    {
        GetWindow<GPTDocumentationWindow>("GPT Documentation Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("GPT Documentation Generator", EditorStyles.boldLabel);

        if (string.IsNullOrEmpty(selectedFolderPath))
        {
            if (GUILayout.Button("Select Folder"))
            {
                selectedFolderPath = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");
            }
            GUILayout.Label($"Selected Folder: {selectedFolderPath}");
        }
        else
        {
            GUILayout.Label($"Folder Selected: {selectedFolderPath}");
        }

        if (GUILayout.Button("Load Scripts"))
        {
            LoadScripts();
        }

        if (scriptPaths.Count > 0)
        {
            GUILayout.Label($"Found {scriptPaths.Count} scripts to process.");
            if (GUILayout.Button("Process Next Script"))
            {
                ProcessNextScript();
            }

            if (summariesGenerated)
            {
                if (GUILayout.Button("Save Markdown Documentation"))
                {
                    SaveMarkdown();
                }
            }
        }
    }

    private void LoadScripts()
    {
        if (string.IsNullOrEmpty(selectedFolderPath))
        {
            Debug.LogError("Please select a folder first.");
            return;
        }

        scriptPaths.Clear();
        scriptPaths.AddRange(Directory.GetFiles(selectedFolderPath, "*.cs", SearchOption.AllDirectories));

        if (scriptPaths.Count == 0)
        {
            Debug.LogWarning("No scripts found in the selected directory.");
        }
        else
        {
            Debug.Log($"Found {scriptPaths.Count} scripts.");
            markdownBuilder.Clear();
            markdownBuilder.AppendLine("# Project Documentation\n");
            currentScriptIndex = 0;
            summariesGenerated = false;
        }
    }

    private async void ProcessNextScript()
    {
        if (currentScriptIndex >= scriptPaths.Count)
        {
            Debug.Log("All scripts processed.");
            summariesGenerated = true;
            return;
        }

        string scriptPath = scriptPaths[currentScriptIndex];
        string scriptContent = File.ReadAllText(scriptPath);

        Debug.Log($"Processing: {scriptPath}");
        string summary = "";// await GetGPTSummary(scriptContent);

        markdownBuilder.AppendLine($"## {Path.GetFileName(scriptPath)}\n");
        markdownBuilder.AppendLine("### File Path\n");
        markdownBuilder.AppendLine($"`{scriptPath}`\n");

        if (!string.IsNullOrEmpty(summary))
        {
            markdownBuilder.AppendLine("### GPT Summary\n");
            markdownBuilder.AppendLine(summary);
        }

        markdownBuilder.AppendLine("\n---\n");
        currentScriptIndex++;

        Debug.Log($"Processed script {currentScriptIndex}/{scriptPaths.Count}.");
    }

    private IEnumerator GetGPTSummary(string fileContent, System.Action<string> onComplete)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";
    
        // Construct JSON payload
        var requestData = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "You are a documentation assistant." },
                new { role = "user", content   = $"Summarize the following Unity script:\n{fileContent}" }
            },
            max_tokens = 150
        };

        string jsonData = JsonUtility.ToJson(requestData);

        // Create UnityWebRequest
        var request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        // Set headers
        request.SetRequestHeader("Content-Type",  "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        // Send request and wait for completion
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;

            // Parse the response
            try
            {
                var    responseData = JsonUtility.FromJson<OpenAIResponse>(result);
                string summary      = responseData.choices[0].message.content;
                onComplete(summary);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing GPT response: {ex.Message}");
                onComplete("Error: Failed to parse GPT response.");
            }
        }
        else
        {
            Debug.LogError($"Error in GPT API call: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
            onComplete($"Error: GPT API call failed with {request.error}");
        }
    }


    private void SaveMarkdown()
    {
        string savePath = Path.Combine(Application.dataPath, "GeneratedDocumentation.md");
        File.WriteAllText(savePath, markdownBuilder.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"Documentation saved at: {savePath}");
    }

    [System.Serializable]
    private class OpenAIResponse
    {
        public Choice[] choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;

            [System.Serializable]
            public class Message
            {
                public string role;
                public string content;
            }
        }
    }
}
