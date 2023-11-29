[System.Serializable]
public struct DanceMoveDirection
{
    public DanceDirection direction;
    public float distance;

    public DanceMoveDirection(DanceDirection direction, float distance)
    {
        this.direction = direction;
        this.distance = distance;
    }
}
