[System.Serializable]
public class DanceJSON
{
    public string danceName;
    public string musicPath;
    public int musicBPM;
    public int musicFirstBeatTime;
    public ChoreographyAction[] choreography;

    public string GetDebugString()
    {
        return $"{danceName} - music:'{musicPath}', BPM:{musicBPM}, actions:{(choreography != null ? choreography.Length : -1)}";
    }
}
