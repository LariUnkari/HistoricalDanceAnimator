using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceScene : MonoBehaviour
{
    public AudioSource _audioSource;

    public string _debugDanceName;

    public DanceData _danceData;

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

        if (_danceData != null && _danceData.danceName != null && _danceData.danceName.Length > 0)
        {
            CreateFormation();
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

    private void CreateFormation()
    {
        foreach (DancerPosition dancerPosition in _danceData.formation.dancerPositions)
            CreateDancer(dancerPosition);
    }

    private void CreateDancer(DancerPosition dancerPosition)
    {
        GameObject dancer = new GameObject($"Dancer_{dancerPosition.role}{dancerPosition.group}");
        GameObject model = Instantiate(GetPawnModelPreset(dancerPosition).model);
        model.transform.parent = dancer.transform;

        dancer.transform.position = dancerPosition.position;
    }

    private PawnModelPreset GetPawnModelPreset(DancerPosition dancerPosition)
    {
        return PawnModelDatabase.GetInstance().GetPreset(PawnModelDatabase.GetPresetKey(dancerPosition.role, dancerPosition.group, dancerPosition.variant));
    }

    private void Play()
    {
        _audioSource.clip = _danceData.music;
        _audioSource.Play();
    }
}
