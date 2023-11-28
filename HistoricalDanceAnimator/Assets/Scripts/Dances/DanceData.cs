using UnityEngine;

[System.Serializable]
public class DanceData
{
    public string danceName;

    public AudioClip music;
    public int bpm;
    public int firstBeatTime;

    public Action[] actions;

    public static DanceData Create(DanceJSON json, AudioClip musicClip)
    {
        DanceData danceData = new DanceData();

        danceData.danceName = json.danceName;
        danceData.bpm = json.musicBPM;
        danceData.firstBeatTime = json.musicFirstBeatTime;
        danceData.music = musicClip;

        // TODO: Build action data from choreography

        return danceData;
    }
}
