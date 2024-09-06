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
    private bool _hasMusicStarted;
    private bool _hasDanceBegun;
    private float _danceTime;
    private float _danceMusicDuration;
    private int _currentBeatIndex;
    private int _previousBeatIndex;
    private int _currentDanceBeatIndex;
    private int _previousDanceBeatIndex;
    private int _currentDanceRepeatIndex;
    private float _currentBPM;
    private float _nextBPM;
    private float _beatDuration;
    private float _beatTime;
    private float _beatT;
    private int _bpmChangeBeatIndex;
    private float _bpmChangeTime;
    private int _bpmChangeSinceBeats;
    private float _bpmChangeSinceTime;

    public bool IsPaused { get { return _isPaused; } }
    public bool HasStarted { get { return _hasMusicStarted; } }
    public string DanceName { get { return _danceData.danceName; } }
    public float DanceTime { get { return _danceTime; } }
    public float DanceDuration { get { return _danceMusicDuration; } }
    public float DanceBPM { get { return _currentBPM; } }
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
        if (_danceData == null || _musicSource == null || !_hasMusicStarted)
            return;

        if (_musicSource.isPlaying)
            UpdateDanceRoutine();
        else if (!_isPaused)
            EndDance();
    }

    public void StartDance()
    {
        _isPaused = false;
        _hasMusicStarted = true;
        _danceTime = 0f;
        _musicSource.clip = _danceData.music;
        _musicSource.Play();
        _currentBPM = _danceData.bpmChanges.initialBPM;

        BeginDanceRoutine();
    }

    private void BeginDanceRoutine()
    {
        Debug.Log("DanceRoutine begins!");

        _currentBeatIndex = -1;
        _previousBeatIndex = -1;
        _currentDanceBeatIndex = -1;
        _previousDanceBeatIndex = -1;
        _currentDanceRepeatIndex = 0;

        _bpmChangeBeatIndex = 0;
        _bpmChangeTime = 0f;
        _bpmChangeSinceBeats = 0;
        _bpmChangeSinceTime = _danceTime;

        _beatDuration = 60f / _currentBPM;
        _beatTime = _danceTime - _currentBeatIndex * _beatDuration;
        _beatT = _beatTime / _beatDuration;

        Debug.Log($"First beat time is ({_danceData.firstBeatTime:F3}s)! Beat duration is {_beatDuration:F3} DanceTime {_danceTime:F3}s");
        _danceMusicDuration = _musicSource.clip.length - _danceData.firstBeatTime;
        _formation.BeginDance();
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

    private void UpdateDanceTime()
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

            if (_danceData.bpmChanges.CheckForBPMChange(_currentBeatIndex, out _nextBPM))
            {
                //int beatsSinceLastChange = currentBeatIndex - _danceData.bpm.lastBPMChangeBeat;
                //currentBeatIndex = Mathf.FloorToInt((danceTime - bpmChangeTime) / beatDuration);
                _currentBPM = _nextBPM;
                _bpmChangeTime = _danceTime - _beatTime;
                _bpmChangeBeatIndex = _currentBeatIndex;
                _beatDuration = 60f / _currentBPM;
                Debug.Log($"BPM Changed to {_currentBPM:F2}, beat duration: {_beatDuration:F3}");
            }

            if (_danceData.danceLength > 0)
            {
                _currentDanceBeatIndex = _currentBeatIndex % _danceData.danceLength;

                if (_currentDanceBeatIndex < _previousDanceBeatIndex)
                    _currentDanceRepeatIndex++;
            }
            else
            {
                _currentDanceBeatIndex = _currentBeatIndex;
            }
        }
    }

    private void UpdateDanceRoutine()
    {
        UpdateDanceTime();

        if ((_danceData.danceLength > 0 && _currentBeatIndex >= _danceData.danceLength) ||
            (_danceData.danceRepeats > 0 && _currentDanceRepeatIndex >= _danceData.danceRepeats))
        {
            EndDance();
            return;
        }

        _formation.DanceUpdate(_danceTime, _currentDanceBeatIndex, _currentDanceRepeatIndex, _beatTime, _beatT, _beatDuration);

        _previousBeatIndex = _currentBeatIndex;
        _previousDanceBeatIndex = _currentDanceBeatIndex;
    }

    public void EndDance()
    {
        _isPaused = false;
        _hasMusicStarted = false;
        _hasDanceBegun = false;
        EndDanceRoutine();
    }

    public void EndDanceRoutine()
    {
        Debug.Log("DanceRoutine ended!");
        _formation.EndDance();
    }
}
