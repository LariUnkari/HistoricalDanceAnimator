using System;
using UnityEngine;

public class DanceUI : MonoBehaviour
{
    public DanceScene _danceScene;

    private void OnGUI()
    {
        if (_danceScene == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, 40));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(new GUIContent(_danceScene.DanceName));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(new GUIContent(_danceScene.DancePart));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 60, 300, 80));
        GUILayout.Label(new GUIContent($"Time: {TimeSpan.FromSeconds(_danceScene.DanceTime).ToString("mm'm 'ss's 'fff'ms'")}"));
        GUILayout.HorizontalSlider(_danceScene.DanceTime / _danceScene.DanceDuration, 0f, 1f, GUILayout.Width(300));
        GUILayout.Label(new GUIContent($"Current BPM: {_danceScene.DanceBPM}"));
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, Screen.height - 100, 300, 80));
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent($"Music volume {Mathf.Floor(_danceScene._musicSource.volume * 100f)}"), GUILayout.Width(140));
        _danceScene._musicSource.volume = GUILayout.HorizontalSlider(_danceScene._musicSource.volume, 0f, 1f, GUILayout.Width(150));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent($"Metronome volume {Mathf.Floor(_danceScene._metronomeSource.volume * 100f)}"), GUILayout.Width(140));
        _danceScene._metronomeSource.volume = GUILayout.HorizontalSlider(_danceScene._metronomeSource.volume, 0f, 1f, GUILayout.Width(150));
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, Screen.height - 60, Screen.width - 20, 40));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent(_danceScene.IsPaused || !_danceScene.HasStarted ? "Play" : "Pause"), GUILayout.Height(40)))
        {
            OnPlayPressed();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OnPlayPressed();
    }

    private void OnPlayPressed()
    {
        if (!_danceScene.HasStarted)
        {
            _danceScene.BeginDanceRoutine();
            return;
        }

        if (!_danceScene.IsPaused)
        {
            Debug.LogWarning($"Pausing dance '{_danceScene.DanceName}'");
            _danceScene.Pause();
        }
        else
        {
            Debug.LogWarning($"Resuming dance '{_danceScene.DanceName}'");
            _danceScene.Play();
        }
    }
}
