using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceScene : MonoBehaviour
{
    public AudioSource _musicSource;
    public AudioSource _metronomeSource;
    public DanceFormation _formation;

    public string _debugDanceName;

    public DanceData _danceData;

    private float _startTime;
    private float _danceTime;
    private int _currentBeatIndex;
    private int _previousBeatIndex;
    private int _bpmChangeBeatIndex;
    private float _bpmChangeTime;
    private int _bpmChangeSinceBeats;
    private float _bpmChangeSinceTime;
    private float _beatDuration;
    private float _beatTime;
    private float _beatT;

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
        if (_musicSource != null && _musicSource.playOnAwake)
            _musicSource.Stop();

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
            if (_danceData != null && _musicSource != null)
            {
                Debug.Log($"Starting to play '{_danceData.danceName}' music");
                Play();
            }
        }
    }

    private void Play()
    {
        _musicSource.clip = _danceData.music;
        _musicSource.Play();

        StartCoroutine(DanceRoutine());
    }

    private IEnumerator DanceRoutine()
    {
        Debug.Log("DanceRoutine begins!");

        _startTime = Time.time + _danceData.firstBeatTime;

        yield return new WaitForSeconds(_danceData.firstBeatTime);

        _danceTime = Time.time - _startTime;

        _currentBeatIndex = 0;
        _previousBeatIndex = -1;

        _bpmChangeBeatIndex = 0;
        _bpmChangeTime = 0f;
        _bpmChangeSinceBeats = 0;
        _bpmChangeSinceTime = _danceTime;

        _beatDuration = 60f / _danceData.bpm.currentBPM;
        _beatTime = _danceTime - _currentBeatIndex * _beatDuration;
        _beatT = _beatTime / _beatDuration;

        Debug.Log($"First beat time is now ({_startTime:F3}s)! Beat duration is {_beatDuration:F3} DanceTime {_danceTime:F3}s");
        _formation.BeginDance();

        while (_musicSource.isPlaying)
        {
            if (_currentBeatIndex > _previousBeatIndex)
            {
                _previousBeatIndex = _currentBeatIndex;

                if (_metronomeSource != null)
                    _metronomeSource.PlayOneShot(_metronomeSource.clip);

                if (_danceData.bpm.CheckForBPMChange(_currentBeatIndex))
                {
                    //int beatsSinceLastChange = currentBeatIndex - _danceData.bpm.lastBPMChangeBeat;
                    //currentBeatIndex = Mathf.FloorToInt((danceTime - bpmChangeTime) / beatDuration);
                    _bpmChangeTime = _danceTime - _beatTime;
                    _bpmChangeBeatIndex = _currentBeatIndex;
                    _beatDuration = 60f / _danceData.bpm.currentBPM;
                    Debug.Log($"BPM Changed to {_danceData.bpm.currentBPM:F2}, beat duration: {_beatDuration:F3}");
                }
            }

            _formation.DanceUpdate(_danceTime, _currentBeatIndex, _beatTime, _beatT, _beatDuration);

            yield return null;

            _danceTime = Time.time - _startTime;
            _bpmChangeSinceTime = _danceTime - _bpmChangeTime;
            _currentBeatIndex = _bpmChangeBeatIndex + Mathf.FloorToInt(_bpmChangeSinceTime / _beatDuration);
            _bpmChangeSinceBeats = _currentBeatIndex - _bpmChangeBeatIndex;
            _beatTime = _bpmChangeSinceTime - _bpmChangeSinceBeats * _beatDuration;
            _beatT = _beatTime / _beatDuration;
        }
    }
}
