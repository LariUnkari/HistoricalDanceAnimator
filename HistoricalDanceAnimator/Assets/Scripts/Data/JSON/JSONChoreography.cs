[System.Serializable]
public class JSONChoreography
{
    public string danceName;
    public string danceType;
    public string danceFamily;
    public string musicPath;
    public float musicBPM;
    public float musicFirstBeatTime;
    public JSONDancerGroup[] groups;
    public JSONDancerPosition[] formation;
    public JSONDancePart[] choreography;

    public string GetDebugString()
    {
        return $"{danceName} - music:'{musicPath}', BPM:{musicBPM}, parts:{(choreography != null ? choreography.Length : -1)}";
    }
}
