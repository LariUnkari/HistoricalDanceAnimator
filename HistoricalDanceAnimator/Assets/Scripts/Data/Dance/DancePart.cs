[System.Serializable]
public class DancePart
{
    public string name;
    public int time;
    public int length;
    public DanceAction[] actions;

    public DancePart(string name, int time, int length, DanceAction[] actions)
    {
        this.name = name;
        this.time = time;
        this.length = length;
        this.actions = actions;
    }
}
