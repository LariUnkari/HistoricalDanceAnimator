[System.Serializable]
public struct DanceVector
{
    public DanceDirection direction;
    public float distance;

    public DanceVector(DanceDirection direction, float distance)
    {
        this.direction = direction;
        this.distance = distance;
    }

    public override string ToString()
    {
        return System.Enum.GetName(typeof(DanceDirection), this.direction);
    }
}
