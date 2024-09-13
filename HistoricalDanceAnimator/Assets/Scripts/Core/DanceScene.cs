using System;
using UnityEngine;

public class DanceScene : BaseScene
{
    public AudioSource _musicSource;
    public AudioSource _metronomeSource;
    public DanceFormation _formation;

    public string _debugDanceName;
    public GameObject _debugDancerPositionPrefab;

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
    private int _previousDanceRepeatIndex;
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
    public string DanceName { get { return _danceData != null ? _danceData.danceName : ""; } }
    public float DanceTime { get { return _danceTime; } }
    public float DanceDuration { get { return _danceMusicDuration; } }
    public int DanceBeat { get { return _currentBeatIndex; } }
    public float DanceBPM { get { return _currentBPM; } }
    public string DancePart { get { return _formation.CurrentPart; } }

    protected override void Awake()
    {
        GameObject go = new GameObject("Formation");
        _formation = go.AddComponent<DanceFormation>();

        base.Awake();
    }

    private void Start()
    {
        if (_musicSource != null && _musicSource.playOnAwake)
            _musicSource.Stop();
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        if (_danceData == null || _musicSource == null || !_hasMusicStarted)
            return;

        if (_musicSource.isPlaying)
            UpdateDance();
        else if (!_isPaused)
            StopDance();
    }

    protected override void OnInitComplete()
    {
        UserData userData = UserData.GetInstance();

        if (userData.danceData != null)
        {
            OnDanceLoadComplete(userData.danceData);
            return;
        }

        JSONDance jsonData;
        if (!DanceDatabase.GetInstance().TryGetDance(_debugDanceName, out jsonData))
        {
            Debug.LogError($"No dance data loaded! Unable to find debug dance data for '{_debugDanceName}'!");
            return;
        }

        DataLoader.GetInstance().LoadDanceJSON(jsonData, OnDanceLoadComplete, OnDanceLoadError);
    }

    private void OnDanceLoadComplete(DanceData danceData)
    {
        Debug.Log($"Dance loaded: '{danceData.danceName}'");
        _danceData = danceData;
        _formation.SetFormation(danceData, _debugDancerPositionPrefab);
    }

    private void OnDanceLoadError(string message)
    {
        Debug.LogError("No dance loaded!");
    }

    public void BeginDance()
    {
        Debug.LogWarning("Dance begins!");

        _isPaused = false;
        _hasMusicStarted = true;
        _danceTime = 0f;

        _musicSource.clip = _danceData.music;
        _danceMusicDuration = _musicSource.clip.length - _danceData.firstBeatTime;
        _musicSource.Play();

        _currentBeatIndex = -1;
        _previousBeatIndex = -1;
        _currentDanceBeatIndex = -1;
        _previousDanceBeatIndex = -1;
        _currentDanceRepeatIndex = 0;

        _currentBPM = _danceData.bpmChanges.initialBPM;
        _bpmChangeBeatIndex = 0;
        _bpmChangeTime = 0f;
        _bpmChangeSinceBeats = 0;
        _bpmChangeSinceTime = _danceTime;

        _beatDuration = 60f / _currentBPM;
        _beatTime = _danceTime - _currentBeatIndex * _beatDuration;
        _beatT = _beatTime / _beatDuration;

        Debug.Log($"First beat time is ({_danceData.firstBeatTime:F3}s)! Beat duration is {_beatDuration:F3} DanceTime {_danceTime:F3}s");

        _formation.OnDanceStarted();
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

    private void UpdateDance()
    {
        UpdateTime();

        if (_danceData.danceRepeats > 0 && _currentDanceBeatIndex < _previousDanceBeatIndex)
            _currentDanceRepeatIndex++;

        if (HasDanceEnded())
        {
            StopDance();
            return;
        }

        if (_currentDanceRepeatIndex > _previousDanceRepeatIndex)
            _formation.OnDanceRepeat(_currentDanceRepeatIndex, _danceData);

        _formation.DanceUpdate(_danceTime, _currentDanceBeatIndex, _beatTime, _beatDuration);

        _previousBeatIndex = _currentBeatIndex;
        _previousDanceBeatIndex = _currentDanceBeatIndex;
        _previousDanceRepeatIndex = _currentDanceRepeatIndex;
    }

    private void UpdateTime()
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
            Debug.Log($"F({Time.frameCount}): Dance progressed to beat {_currentBeatIndex} at time {_danceTime:F3}, Current beat t={_beatT:F3}, time={_beatTime:F3}, duration={_beatDuration:F3}");

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

                Debug.Log($"F({Time.frameCount}): BPM Changed to {_currentBPM:F2}, beat duration: {_beatDuration:F3}");
            }

            _currentDanceBeatIndex = _danceData.danceLength > 0 ? _currentBeatIndex % _danceData.danceLength : _currentBeatIndex;
        }

        _formation.UpdateDanceTime(_danceTime);
    }

    private bool HasDanceEnded()
    {
        if (_danceData.danceRepeats > 0 && _currentDanceRepeatIndex < _danceData.danceRepeats)
            return false;

        if (_danceData.danceLength > 0 && _currentBeatIndex < _danceData.danceLength)
            return false;

        return true;
    }

    public void StopDance()
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
