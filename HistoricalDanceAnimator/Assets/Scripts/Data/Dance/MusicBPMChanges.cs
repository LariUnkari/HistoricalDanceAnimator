using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MusicBPMChanges
{
    public float initialBPM;
    private Dictionary<int, float> bpmChanges = new Dictionary<int, float>();

    public bool CheckForBPMChange(int beatIndex, out float newBPM)
    {
        if (bpmChanges.TryGetValue(beatIndex, out newBPM))
            return true;

        newBPM = 0f;
        return false;
    }

    public void SetInitialBPM(float bpm)
    {
        initialBPM = bpm;
    }

    public void AddBPMChange(int beatIndex, float bpm)
    {
        bpmChanges.Add(beatIndex, bpm);
    }
}
