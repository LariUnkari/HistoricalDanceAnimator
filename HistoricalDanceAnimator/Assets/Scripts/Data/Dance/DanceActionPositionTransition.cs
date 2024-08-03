using UnityEngine;

[System.Serializable]
public class DanceActionPositionTransition
{
    public float time;
    public float duration;
    public float endTime;
    public Vector3 vector;

    public DanceActionPositionTransition(float time, float duration, Vector3 vector)
    {
        this.time = time;
        this.duration = duration;
        this.endTime = time + duration;
        this.vector = vector;
    }
}
