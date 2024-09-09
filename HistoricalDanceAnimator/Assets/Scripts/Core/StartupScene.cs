using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScene : BaseScene
{
    public string _danceSceneName = "DanceScene";

    public void LoadDanceScene(DanceData danceData)
    {
        UserData.GetInstance().danceData = danceData;
        SceneManager.LoadScene(_danceSceneName);
    }

    protected override void OnInitComplete()
    {
        
    }
}
