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

    private Dictionary<string, DanceData> danceDictionary;

    private void Awake()
    {
        danceDictionary = new Dictionary<string, DanceData>();
    }

    public void AddDance(DanceData danceData)
    {
        danceDictionary.Add(danceData.danceName, danceData);
    }

    public bool TryGetDance(string danceName, out DanceData danceData)
    {
        return danceDictionary.TryGetValue(danceName, out danceData);
    }
}
