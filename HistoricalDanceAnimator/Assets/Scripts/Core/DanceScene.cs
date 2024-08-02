using System;
using UnityEngine;

public class DanceScene : MonoBehaviour
{
    public AudioSource _musicSource;
    public AudioSource _metronomeSource;
    public DanceFormation _formation;

    public string _debugDanceName;

    private DanceData _danceData;

    private bool _isPaused;
    private bool _hasStarted;
    private bool _hasDanceBegun;
    private float _danceTime;
    private float _danceMusicDuration;
    private int _currentBeatIndex;
    private int _previousBeatIndex;
    private int _bpmChangeBeatIndex;
    private float _bpmChangeTime;
    private int _bpmChangeSinceBeats;
    private float _bpmChangeSinceTime;
    private float _beatDuration;
    private float _beatTime;
    private float _beatT;

    public bool IsPaused { get { return _isPaused; } }
    public bool HasStarted { get { return _hasStarted; } }
    public string DanceName { get { return _danceData.danceName; } }
    public float DanceTime { get { return _danceTime; } }
    public float DanceDuration { get { return _danceMusicDuration; } }
    public float DanceBPM { get { return _danceData.bpm.currentBPM; } }
    public string DancePart { get { return _formation.CurrentPart; } }

    private void Awake()
    {
        GameObject go = new GameObject("Formation");
        _formation = go.AddComponent<DanceFormation>();

        DanceDatabase database = DanceDatabase.GetInstance();

        UserData userData = UserData.GetInstance();
        if (userData.danceName.Length == 0)
            userData.danceName = _debugDanceName;

        if (database.TryGetDance(userData.danceName, out DanceData danceData))
        {
            _danceData = danceData;
            Debug.Log($"Dance loaded: '{danceData.danceName}'");
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
        if (_danceData == null || _musicSource == null)
            return;

        if (_musicSource.isPlaying)
            UpdateDanceRoutine();
    }

    public void BeginDanceRoutine()
    {
        Debug.Log("DanceRoutine begins!");

        _isPaused = false;
        _hasStarted = true;
        _musicSource.clip = _danceData.music;

        _currentBeatIndex = -1;
        _previousBeatIndex = -1;

        _bpmChangeBeatIndex = 0;
        _bpmChangeTime = 0f;
        _bpmChangeSinceBeats = 0;
        _bpmChangeSinceTime = _danceTime;

        _beatDuration = 60f / _danceData.bpm.currentBPM;
        _beatTime = _danceTime - _currentBeatIndex * _beatDuration;
        _beatT = _beatTime / _beatDuration;

        Debug.Log($"First beat time is ({_danceData.firstBeatTime:F3}s)! Beat duration is {_beatDuration:F3} DanceTime {_danceTime:F3}s");
        _danceMusicDuration = _musicSource.clip.length - _danceData.firstBeatTime;
        _formation.BeginDance();
        _musicSource.Play();
    }

    public void Play()
    {
        _isPaused = false;
        _musicSource.Play();
        _formation.OnResume();
    }

    public void Pause()
    {
        _isPaused = true;
        _musicSource.Pause();
        _formation.OnPause();
    }

    private void UpdateDanceRoutine()
    {
        _danceTime = _musicSource.time - _danceData.firstBeatTime;

        if (!_hasDanceBegun)
        {
            if (_danceTime < 0f)
                return;

            _hasDanceBegun = true;
        }
        else
        {
            _bpmChangeSinceTime = _danceTime - _bpmChangeTime;
            _currentBeatIndex = _bpmChangeBeatIndex + Mathf.FloorToInt(_bpmChangeSinceTime / _beatDuration);
            _bpmChangeSinceBeats = _currentBeatIndex - _bpmChangeBeatIndex;
            _beatTime = _bpmChangeSinceTime - _bpmChangeSinceBeats * _beatDuration;
            _beatT = _beatTime / _beatDuration;
        }

        if (_currentBeatIndex > _previousBeatIndex)
        {
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

        _previousBeatIndex = _currentBeatIndex;
    }
}
