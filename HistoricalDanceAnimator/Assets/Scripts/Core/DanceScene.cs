using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceScene : MonoBehaviour
{
    public AudioSource _audioSource;
    public DanceFormation _formation;

    public string _debugDanceName;

    public DanceData _danceData;

    private void Awake()
    {
        GameObject go = new GameObject("Formation");
        _formation = go.AddComponent<DanceFormation>();

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

        // Check dance data is valid before setting up formation (Unity Inspector can create invalid data if valid data was not set)
        if (_danceData != null && _danceData.danceName != null && _danceData.danceName.Length > 0)
        {
            _formation.SetFormation(_danceData);
        }
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

        StartCoroutine(DanceRoutine());
    }

    private IEnumerator DanceRoutine()
    {
        Debug.Log("DanceRoutine begins!");

        float startTime = Time.time + _danceData.firstBeatTime;
        float beatDuration = 60f / _danceData.bpm;

        yield return new WaitForSeconds(_danceData.firstBeatTime);

        float danceTime = Time.time - startTime;
        float beatT = 0f;
        float beatTime = 0f;
        int beatIndex = 0;

        Debug.Log($"First beat time is now ({startTime:F3}s)! Beat duration is {beatDuration:F3} DanceTime {danceTime:F3}s");
        _formation.BeginDance();

        while (_audioSource.isPlaying)
        {
            _formation.DanceUpdate(danceTime, beatIndex, beatTime, beatT, beatDuration);

            yield return null;

            danceTime = Time.time - startTime;
            beatIndex = Mathf.FloorToInt(danceTime / beatDuration);
            beatTime = danceTime - beatIndex * beatDuration;
            beatT = beatTime / beatDuration;
        }
    }
}
