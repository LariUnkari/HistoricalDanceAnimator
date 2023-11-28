using UnityEngine;

public class DataPayloadJSON
{
    public string path;
    public string data;

    public DataPayloadJSON(string path)
    {
        this.path = path;
        data = null;
    }
}
