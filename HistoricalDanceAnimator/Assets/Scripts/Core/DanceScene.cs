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
            UpdateDanceRoutine();
        else if (!_isPaused)
            EndDance();
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

            _currentDanceBeatIndex = _danceData.danceLength > 0 ? _currentBeatIndex % _danceData.danceLength : _currentBeatIndex;
        }
    }

    private void UpdateDanceRoutine()
    {
        UpdateDanceTime();

        if (HasDanceEnded())
        {
            EndDance();
            return;
        }

        if (_currentDanceBeatIndex < _previousDanceBeatIndex)
            RepeatDance();

        _formation.DanceUpdate(_danceTime, _currentDanceBeatIndex, _currentDanceRepeatIndex, _beatTime, _beatT, _beatDuration);

        _previousBeatIndex = _currentBeatIndex;
        _previousDanceBeatIndex = _currentDanceBeatIndex;
    }

    private bool HasDanceEnded()
    {
        if (_danceData.danceRepeats > 0 && _currentDanceRepeatIndex < _danceData.danceRepeats)
            return false;

        if (_danceData.danceLength > 0 && _currentBeatIndex < _danceData.danceLength)
            return false;

        return true;
    }

    private void RepeatDance()
    {
        _currentDanceRepeatIndex++;
        string key;

        foreach (DancerPosition position in _formation._dancerPositions)
        {
            if (_danceData.danceProgression == DanceProgression.Line_AB)
            {
                key = null;

                if (position.Role.group.id == "A")
                {
                    position.SetPositionIndex++;

                    if (position.SetPositionIndex == _formation.SetLength - 1)
                        key = DancerRole.GetRoleKey("inactive", position.Role.id);
                }
                else if (position.Role.group.id == "B")
                {
                    position.SetPositionIndex--;

                    if (position.SetPositionIndex == 0)
                        key = DancerRole.GetRoleKey("inactive", position.Role.id);
                }
                else if (position.Role.group.id == "inactive")
                {
                    if (position.SetPositionIndex == 0)
                        key = DancerRole.GetRoleKey("A", position.Role.id);
                    else if (position.SetPositionIndex == _formation.SetLength - 1)
                        key = DancerRole.GetRoleKey("B", position.Role.id);
                }

                if (key != null)
                {
                    DancerRole role;
                    if (_danceData.TryGetRole(key, out role))
                    {
                        Debug.LogWarning($"Switching dancer {position.DancerIndex}/{_formation._dancerPositions.Length} at set position {position.SetPositionIndex}/{_formation.SetLength} role {position.Role.key} to role {role.key}");
                        position.SetRole(role);
                    }
                    else
                    {
                        Debug.LogError($"Error in switching dancer {position.DancerIndex}/{_formation._dancerPositions.Length} at set position {position.SetPositionIndex}/{_formation.SetLength} role {position.Role.key} to role {key}!");
                    }
                }
            }
        }
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
