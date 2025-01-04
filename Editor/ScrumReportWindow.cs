using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;

namespace THEBADDEST
{
    public class ScrumReportWindow : EditorWindow
    {
        private string date = "";
        private string time = "";
        private string gameName = "";
        private List<string> yesterday = new List<string>();
        private List<string> today = new List<string>();
        private List<string> blockers = new List<string>();
        private string notes = "";

        private Vector2 scrollPosition = Vector2.zero;

        private ReorderableList yesterdayList;
        private ReorderableList todayList;
        private ReorderableList blockersList;

        // Slack integration fields
        private string webhookUrl = "YOUR_WEBHOOK_URL_HERE";
        private string slackChannel = "#your-channel";
        private string slackUsername = "UnityBot";
        private SlackMessageSender slackMessageSender;

        [MenuItem("Window/Scrum Report")]
        public static void ShowWindow()
        {
            GetWindow<ScrumReportWindow>("Scrum Report");
        }

        private void OnEnable()
        {
            // Automatically set date and time to current date and time when window is enabled
            date = DateTime.Now.ToString("dd/MM/yyyy");
            time = DateTime.Now.ToString("hh:mm tt");
            if (EditorPrefs.HasKey("ScrumReportGameName"))
            {
                gameName = EditorPrefs.GetString("ScrumReportGameName");
            }

            // Initialize SlackMessageSender
            slackMessageSender = new SlackMessageSender(webhookUrl, slackChannel, slackUsername);

            // Create reorderable lists
            yesterdayList = new ReorderableList(yesterday, typeof(string), true, true, true, true);
            yesterdayList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Yesterday's Tasks");
            yesterdayList.drawElementCallback = (rect, index, isActive, isFocused) => { yesterday[index] = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), yesterday[index]); };
            todayList = new ReorderableList(today, typeof(string), true, true, true, true);
            todayList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Today's Tasks");
            todayList.drawElementCallback = (rect, index, isActive, isFocused) => { today[index] = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), today[index]); };
            blockersList = new ReorderableList(blockers, typeof(string), true, true, true, true);
            blockersList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Blockers");
            blockersList.drawElementCallback = (rect, index, isActive, isFocused) => { blockers[index] = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), blockers[index]); };
        }

        private void OnGUI()
        {
            GUILayout.Label("Daily Scrum Report", EditorStyles.boldLabel);

            // Display date and time fields as read-only text
            date = EditorGUILayout.TextField("Date", date);
            time = EditorGUILayout.TextField("Time", time);
            GUILayout.Space(10);
            gameName = EditorGUILayout.TextField("Game Name", gameName);
            GUILayout.Space(10);

            // Slack fields
            webhookUrl = EditorGUILayout.TextField("Slack Webhook URL", webhookUrl);
            slackChannel = EditorGUILayout.TextField("Slack Channel", slackChannel);
            slackUsername = EditorGUILayout.TextField("Slack Username", slackUsername);
            GUILayout.Space(10);

            // Scroll view for reorderable list of yesterday's tasks
            yesterdayList.DoLayoutList();
            GUILayout.Space(10);

            // Scroll view for reorderable list of today's tasks
            todayList.DoLayoutList();
            GUILayout.Space(10);

            // Scroll view for reorderable list of blockers
            blockersList.DoLayoutList();
            GUILayout.Space(10);

            // Text area for notes
            GUILayout.Label("Notes", EditorStyles.boldLabel);
            notes = EditorGUILayout.TextArea(notes, GUILayout.Height(100));
            GUILayout.Space(10);

            // Button to update report
            if (GUILayout.Button("Generate and Copy"))
            {
                UpdateReport();
            }

            // Button to send report to Slack
            if (GUILayout.Button("Send to Slack"))
            {
                SendReportToSlack();
            }
        }

        private void UpdateReport()
        {
            string report = GenerateReport();

            // Copy report to clipboard
            EditorGUIUtility.systemCopyBuffer = report;
            EditorPrefs.SetString("ScrumReportGameName", gameName);
            // Log report
            Debug.Log(report);
            Debug.Log("Copied");
        }

        private void SendReportToSlack()
        {
            string report = GenerateReport();
            slackMessageSender.SetWebhookUrl(webhookUrl);
            slackMessageSender.SetChannel(slackChannel);
            slackMessageSender.SetUsername(slackUsername);
            //EditorCoroutineUtility.StartCoroutine(slackMessageSender.PostMessage(report), this);
        }

        private string GenerateReport()
        {
            string yesterdayTasks = "";
            foreach (string task in yesterday)
            {
                yesterdayTasks += $"- {task}\n";
            }

            string todayTasks = "";
            foreach (string task in today)
            {
                todayTasks += $"- {task}\n";
            }

            string blockersText = "";
            foreach (string blocker in blockers)
            {
                blockersText += $"- {blocker}\n";
            }

            string report = $"Daily Scrum Report\n" +
                            $"Date: [{date}]\n" +
                            $"Time: [{time}]\n\n" +
                            $"GameName: {gameName}\n\n" +
                            $"1. What did I do yesterday?\n{yesterdayTasks}\n" +
                            $"2. What will I do today?\n{todayTasks}\n" +
                            $"3. Any blockers or impediments?\n{blockersText}\n\n" +
                            $"Notes:\n{notes}";

            return report;
        }

        // Nested class for sending Slack messages
        public class SlackMessageSender
        {
            private string webhookUrl;
            private string channel;
            private string username;

            public SlackMessageSender(string webhookUrl, string channel, string username)
            {
                this.webhookUrl = webhookUrl;
                this.channel = channel;
                this.username = username;
            }

            public void SetWebhookUrl(string webhookUrl)
            {
                this.webhookUrl = webhookUrl;
            }

            public void SetChannel(string channel)
            {
                this.channel = channel;
            }

            public void SetUsername(string username)
            {
                this.username = username;
            }

            public IEnumerator PostMessage(string message)
            {
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    Debug.LogError("Slack webhook URL is not set.");
                    yield break;
                }

                SlackPayload payload = new SlackPayload
                {
                    text = message,
                    channel = this.channel,
                    username = this.username
                };

                string jsonPayload = JsonUtility.ToJson(payload);

                using (UnityWebRequest www = new UnityWebRequest(webhookUrl, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                    www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");

                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Error sending message to Slack: " + www.error);
                    }
                    else
                    {
                        Debug.Log("Message sent to Slack successfully!");
                    }
                }
            }

            [System.Serializable]
            private class SlackPayload
            {
                public string text;
                public string channel;
                public string username;
            }
        }
    }
}

