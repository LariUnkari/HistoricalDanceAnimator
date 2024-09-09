[System.Serializable]
public class JSONDance
{
    public string danceName;
    public string danceFamily;
    public JSONDanceSet danceSet;
    public JSONDanceProgression danceProgression;

    public string musicPath;
    public float musicBPM;
    public JSONMusicBeatTiming[] musicBeatTimings;
    public float musicFirstBeatTime;

    public JSONDancerGroup[] groups;
    public JSONDancerGroupPosition[] formation;
    public JSONDancePart[] choreography;

    public string GetDebugString()
    {
        return $"{danceName} - music:'{musicPath}', BPM:{musicBPM}, choreography parts:{(choreography != null ? choreography.Length : -1)}";
    }
}
