[System.Serializable]
public class DanceMovement
{
    public DanceVector[] directions;

    public DanceMovement(DanceVector[] directions)
    {
        this.directions = directions;
    }

    public override string ToString()
    {
        return this.directions != null ? string.Join(",", this.directions) : "*None*";
    }
}
