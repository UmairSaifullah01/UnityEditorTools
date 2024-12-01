using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace THEBADDEST.Shortcuts
{
    public static class AutoSaveShortcut
    {
        [Shortcut("THEBADDEST/Save Project", KeyCode.S, ShortcutModifiers.Action)]
        public static void SaveProjectShortcut()
        {
            SaveProject();
        }

        private static void SaveProject()
        {
            Debug.Log("Saving project...");
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("File/Save");
            EditorApplication.ExecuteMenuItem("File/Save Project");
            Debug.Log("Project saved successfully!");
        }
    }
}
