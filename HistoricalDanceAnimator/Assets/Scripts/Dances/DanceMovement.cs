[System.Serializable]
public class DanceMovement
{
    public DanceMoveDirection[] directions;

    public DanceMovement(DanceMoveDirection[] directions)
    {
        this.directions = directions;
    }
}
