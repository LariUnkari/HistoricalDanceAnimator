using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupHandler : MonoBehaviour
{
    public DataLoader dataLoader;

    public string danceSceneName;

    private string selectedDance;
    private bool doLoadDance;

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

    private void Start()
    {
        selectedDance = "";
        doLoadDance = false;

        if (dataLoader)
            dataLoader.LoadData();
    }

    private void OnDataLoaded()
    {
        Debug.Log("Done loading dance data");
    }

    private void OnLoadDance(string danceName)
    {
        UserData.GetInstance().danceName = danceName;
        SceneManager.LoadScene(danceSceneName);
    }

    private void Update()
    {
        if (doLoadDance)
        {
            OnLoadDance(selectedDance);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        if (!dataLoader)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Initializing...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else if (!dataLoader.IsDoneLoading)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Loading...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("List of dances");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            foreach (string key in DanceDatabase.GetInstance().GetDanceNames())
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

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

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = selectedDance.Length > 0;
            if (GUILayout.Button(selectedDance.Length > 0 ? "Load dance '" + selectedDance + "'" : "Select a dance"))
            {
                doLoadDance = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
        }

        GUILayout.EndArea();
    }
}
