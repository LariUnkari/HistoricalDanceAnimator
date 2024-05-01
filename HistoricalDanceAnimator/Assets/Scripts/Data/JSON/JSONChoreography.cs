[System.Serializable]
public class JSONChoreography
{
    public string danceName;
    public string danceType;
    public string musicPath;
    public float musicBPM;
    public float musicFirstBeatTime;
    public JSONDancerGroup[] groups;
    public JSONDancerPosition[] formation;
    public JSONDanceAction[] choreography;

    public string GetDebugString()
    {
        return $"{danceName} - music:'{musicPath}', BPM:{musicBPM}, actions:{(choreography != null ? choreography.Length : -1)}";
    }
}
