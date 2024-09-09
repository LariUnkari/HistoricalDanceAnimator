using System.Collections.Generic;
using UnityEngine;

public class DanceDatabase : MonoBehaviour
{
    private static DanceDatabase s_instance;

    public static DanceDatabase GetInstance()
    {
        if (s_instance)
            return s_instance;

        return CreateSingleton();
    }

    private static DanceDatabase CreateSingleton()
    {
        GameObject go = new GameObject("DanceDatabase");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<DanceDatabase>();
        return s_instance;
    }

    private Dictionary<string, JSONDanceData> _danceDictionary;

    private void Awake()
    {
        _danceDictionary = new Dictionary<string, JSONDanceData>();
    }

    public void AddDance(JSONDanceData danceData)
    {
        _danceDictionary.Add(danceData.danceName, danceData);
    }

    public bool TryGetDance(string danceName, out JSONDanceData danceData)
    {
        return _danceDictionary.TryGetValue(danceName, out danceData);
    }

    public ICollection<string> GetDanceNames()
    {
        return _danceDictionary.Keys;
    }
}
