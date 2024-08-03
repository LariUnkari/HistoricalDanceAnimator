using System.Collections;
using UnityEngine;

public class StartupUI : MonoBehaviour
{
    public StartupHandler startupHandler;
    public DataLoader dataLoader;

    private string selectedDance;
    private bool loadDanceSceneClicked;

    private float panelWidth;
    private float panelHeight;

    private string inputText;
    private string importMessage;
    private bool isImporting;
    private IEnumerator importRoutine;
    private JSONChoreography importChoreography;

    private void OnEnable()
    {
        if (dataLoader)
            dataLoader.OnCompletionCallback += OnDataLoaded;
    }

    private void OnDisable()
    {
        if (dataLoader)
            dataLoader.OnCompletionCallback -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        Debug.Log("Done loading dance data");
    }

    private void Start()
    {
        inputText = "";
        importMessage = "";
        selectedDance = "";
        loadDanceSceneClicked = false;

        if (dataLoader)
            dataLoader.LoadData();
    }

    private void Update()
    {
        if (isImporting)
        {
            if (importRoutine == null)
            {
                importRoutine = ImportRoutine(importChoreography);
                StartCoroutine(importRoutine);
            }
        }

        if (loadDanceSceneClicked)
        {
            loadDanceSceneClicked = false;
            if (importRoutine != null) StopCoroutine(importRoutine);
            startupHandler.LoadDanceScene(selectedDance);
        }
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

        if (!dataLoader)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Initializing...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if (!dataLoader.IsDoneLoading)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Loading...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (dataLoader && dataLoader.IsDoneLoading)
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
                importChoreography = JsonUtility.FromJson<JSONChoreography>(inputText);
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

    private IEnumerator ImportRoutine(JSONChoreography choreography)
    {
        yield return dataLoader.ImportDanceJSON(choreography, OnImportProgress, OnImportComplete, OnImportError);
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

    private void OnImportComplete()
    {
        importMessage = $"Import successful, added '{importChoreography.danceName}'";
    }
}
