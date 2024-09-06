[System.Serializable]
public class JSONChoreography
{
    public string danceName;
    public string danceFamily;
    public string danceSet;
    public string danceGroupType;
    public JSONGroupCount dancerGroupCount;
    public string danceProgression;
    public int danceRepeats;
    public int danceLength;
    public string musicPath;
    public float musicBPM;
    public JSONMusicBeatTiming[] musicBeatTimings;
    public float musicFirstBeatTime;
    public JSONDancerGroup[] groups;
    public JSONGroupPosition[] formation;
    public JSONDancePart[] choreography;

    public string GetDebugString()
    {
        return $"{danceName} - music:'{musicPath}', BPM:{musicBPM}, parts:{(choreography != null ? choreography.Length : -1)}";
    }
}
