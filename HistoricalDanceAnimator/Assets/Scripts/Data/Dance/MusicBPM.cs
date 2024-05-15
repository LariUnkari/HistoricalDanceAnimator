using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MusicBPM
{
    public float initialBPM;
    public float currentBPM;

    private float nextBPM;
    private Dictionary<int, float> bpmChanges = new Dictionary<int, float>();

    public bool CheckForBPMChange(int beatIndex)
    {
        if (bpmChanges.TryGetValue(beatIndex, out nextBPM))
        {
            currentBPM = nextBPM;
            return true;
        }

        return false;
    }

    public void SetInitialBPM(float bpm)
    {
        initialBPM = bpm;
        currentBPM = bpm;
    }

    public void AddBPMChange(int beatIndex, float bpm)
    {
        bpmChanges.Add(beatIndex, bpm);
    }
}
