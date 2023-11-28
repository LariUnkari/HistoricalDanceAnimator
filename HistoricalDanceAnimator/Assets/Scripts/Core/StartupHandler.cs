using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupHandler : MonoBehaviour
{
    public DataLoader dataLoader;

    public string danceSceneName;

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
        if (dataLoader)
            dataLoader.LoadData();
    }

    private void OnDataLoaded()
    {
        SceneManager.LoadScene(danceSceneName);
    }
}
