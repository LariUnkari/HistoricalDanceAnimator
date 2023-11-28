using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceScene : MonoBehaviour
{
    public AudioSource _audioSource;

    public string _debugDanceName;
    private DanceData _danceData;

    private void Awake()
    {
        DanceDatabase database = DanceDatabase.GetInstance();

        if (database.TryGetDance(_debugDanceName, out DanceData danceData))
        {
            _danceData = danceData;
            Debug.Log($"Debug dance loaded: '{danceData.danceName}'");
        }
    }

    private void Start()
    {
        if (_audioSource != null && _audioSource.playOnAwake)
            _audioSource.Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_danceData != null && _audioSource != null)
            {
                Debug.Log($"Starting to play '{_danceData.danceName}' music");
                Play();
            }
        }
    }

    private void Play()
    {
        _audioSource.clip = _danceData.music;
        _audioSource.Play();
    }
}
