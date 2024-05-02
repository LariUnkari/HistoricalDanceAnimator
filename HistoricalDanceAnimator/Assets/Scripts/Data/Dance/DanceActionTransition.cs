[System.Serializable]
public class DanceActionTransition 
{
    public float time;
    public float duration;
    public DanceDirection direction;
    public float amount;

    public DanceActionTransition(float time, float duration, DanceDirection direction, float amount)
    {
        this.time = time;
        this.duration = duration;
        this.direction = direction;
        this.amount = amount;
    }
}
