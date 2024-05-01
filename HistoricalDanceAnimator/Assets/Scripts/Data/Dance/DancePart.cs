[System.Serializable]
public class DancePart
{
    public string name;
    public int time;
    public DanceAction[] actions;

    public DancePart(string name, int time, DanceAction[] actions)
    {
        this.name = name;
        this.time = time;
        this.actions = actions;
    }
}
