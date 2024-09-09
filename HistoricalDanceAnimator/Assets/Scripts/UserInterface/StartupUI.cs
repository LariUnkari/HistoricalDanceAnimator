using System.Collections;
using UnityEngine;

public class StartupUI : MonoBehaviour
{
    public StartupScene startupHandler;

    private string selectedDance;
    private bool loadDanceSceneClicked;

    private float panelWidth;
    private float panelHeight;

    private string inputText;
    private string importMessage;
    private bool isImporting;
    private IEnumerator importRoutine;
    private JSONDanceData importChoreography;

    private void Start()
    {
        inputText = "";
        importMessage = "";
        selectedDance = "";
        loadDanceSceneClicked = false;
    }

    private void Update()
    {
        if (isImporting)
        {
            if (importRoutine == null)
            {
                importRoutine = ImportDanceRoutine(importChoreography);
                StartCoroutine(importRoutine);
            }
        }

        if (loadDanceSceneClicked)
        {
            loadDanceSceneClicked = false;

            if (importRoutine != null)
                StopCoroutine(importRoutine);

            if (DataLoader.GetInstance() != null)
            {
                if (DanceDatabase.GetInstance().TryGetDance(selectedDance, out JSONDanceData jsonData))
                    DataLoader.GetInstance().LoadDanceJSON(jsonData, OnDanceDataLoaded, OnDanceDataLoadError);
                else
                    Debug.LogError($"Unable to find data for dance '{selectedDance}'!");
            }
        }
    }

    private void OnDanceDataLoaded(DanceData danceData)
    {
        Debug.Log($"Done loading dance data '{danceData.danceName}'");
        startupHandler.LoadDanceScene(danceData);
    }

    private void OnDanceDataLoadError(string message)
    {
        Debug.LogError($"Unsuccessful at loading data for dance '{selectedDance}'!\nError message: '{message}'");
    }

    private void OnGUI()
    {
        panelWidth = (Screen.width >> 1) - 15;
        panelHeight = Screen.height - 120;

        DrawDanceList();
        DrawJSONImporter();

        GUILayout.BeginArea(new Rect(20, Screen.height - 60, Screen.width - 40, 40));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.enabled = selectedDance.Length > 0;
        if (GUILayout.Button(selectedDance.Length > 0 ? "Load dance '" + selectedDance + "'" : "Select a dance", GUILayout.Height(40)))
        {
            loadDanceSceneClicked = true;
        }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawDanceList()
    {
        GUILayout.BeginArea(new Rect(10, 10, panelWidth, panelHeight), GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("List of dances");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (DataLoader.GetInstance() == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Initializing...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if (!DataLoader.GetInstance().IsDoneLoading)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Loading...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (DataLoader.GetInstance() != null && DataLoader.GetInstance().IsDoneLoading)
        {
            foreach (string key in DanceDatabase.GetInstance().GetDanceNames())
            {
                if (GUILayout.Button(new GUIContent(key)))
                {
                    if (selectedDance == key)
                    {
                        selectedDance = "";
                    }
                    else
                    {
                        selectedDance = key;
                    }
                }
            }
        }

        GUILayout.EndArea();
    }

    private Vector2 inputScrollPosition;

    private void DrawJSONImporter()
    {
        GUILayout.BeginArea(new Rect(panelWidth + 20, 10, panelWidth, panelHeight), GUI.skin.box);

        inputScrollPosition = GUILayout.BeginScrollView(inputScrollPosition, false, true, GUILayout.Height(panelHeight - 30));
        inputText = GUILayout.TextArea(inputText, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUI.enabled = !isImporting;
        if (GUILayout.Button(new GUIContent("Import dance from JSON"), GUILayout.Width(200)))
        {
            importMessage = "Checking JSON...";

            try
            {
                importChoreography = JsonUtility.FromJson<JSONDanceData>(inputText);
            }
            catch
            {
                importChoreography = null;
            }

            if (importChoreography == null)
            {
                importMessage = "Invalid JSON";
            }
            else
            {
                importMessage = "Valid JSON, importing...";
                isImporting = true;
            }
        }
        GUI.enabled = true;
        GUILayout.Label(importMessage, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private IEnumerator ImportDanceRoutine(JSONDanceData data)
    {
        if (DataLoader.GetInstance() != null)
            yield return DataLoader.GetInstance().ImportDanceJSON(data, OnImportProgress, OnImportComplete, OnImportError);
        else
            yield return null;

        importRoutine = null;
        isImporting = false;
    }

    private void OnImportProgress(string message)
    {
        importMessage = message;
    }

    private void OnImportError(string message)
    {
        importMessage = message;
    }

    private void OnImportComplete(DanceData danceData)
    {
        importMessage = $"Import successful, added '{danceData.danceName}'";
    }
}
