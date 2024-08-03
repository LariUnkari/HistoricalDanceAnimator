[System.Serializable]
public class DanceActionRotationTransition 
{
    public float time;
    public float duration;
    public float endTime;
    public DanceDirection direction;
    public float amount;

    public DanceActionRotationTransition(float time, float duration, DanceDirection direction, float amount)
    {
        this.time = time;
        this.duration = duration;
        this.endTime = time + duration;
        this.direction = direction;
        this.amount = amount;
    }
}
