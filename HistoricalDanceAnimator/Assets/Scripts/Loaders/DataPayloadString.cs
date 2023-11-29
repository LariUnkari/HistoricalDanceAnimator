using UnityEngine;

public class DataPayloadString
{
    public string path;
    public string data;

    public DataPayloadString(string path)
    {
        this.path = path;
        data = null;
    }
}
