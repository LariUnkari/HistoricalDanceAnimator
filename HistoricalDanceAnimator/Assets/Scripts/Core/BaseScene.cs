using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseScene : MonoBehaviour
{
    public string _dataSceneName = "Data";

    protected bool _isInitialized;

    private static AsyncOperation s_loadDataSceneOperation;

    protected virtual void Awake()
    {
        _isInitialized = false;

        // If DataLoader exists, init is already completed
        if (DataLoader.GetInstance() != null)
        {
            InitComplete();
            return;
        }

        // If already in loading process, just wait for it to finish
        if (s_loadDataSceneOperation != null)
            return;

        StartCoroutine(DataSceneLoadRoutine());
    }

    private void InitComplete()
    {
        _isInitialized = true;
        OnInitComplete();
    }

    protected abstract void OnInitComplete();

    private IEnumerator DataSceneLoadRoutine()
    {
        s_loadDataSceneOperation = SceneManager.LoadSceneAsync(_dataSceneName, LoadSceneMode.Additive);

        while (DataLoader.GetInstance() == null)
            yield return new WaitForSeconds(0.25f);

        while (!DataLoader.GetInstance().IsDoneLoading)
            yield return new WaitForSeconds(0.25f);

        s_loadDataSceneOperation = null;
        InitComplete();
    }
}
