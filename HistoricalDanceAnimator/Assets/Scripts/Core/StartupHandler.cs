using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupHandler : MonoBehaviour
{
    public string danceSceneName;

    public void LoadDanceScene(string danceName)
    {
        UserData.GetInstance().danceName = danceName;
        SceneManager.LoadScene(danceSceneName);
    }
}
