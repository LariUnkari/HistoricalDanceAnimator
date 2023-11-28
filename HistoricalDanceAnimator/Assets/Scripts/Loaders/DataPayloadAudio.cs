using UnityEngine;

public class DataPayloadAudio
{
    public string path;
    public AudioType type;
    public AudioClip clip;

    public DataPayloadAudio(string path, AudioType type)
    {
        this.path = path;
        this.type = type;
        clip = null;
    }
}
