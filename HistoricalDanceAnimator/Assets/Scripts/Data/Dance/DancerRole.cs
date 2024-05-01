using System.Collections.Generic;

[System.Serializable]
public class DancerRole
{
    public string id;

    [System.NonSerialized]
    public string key;

    [System.NonSerialized]
    public DancerGroup group;

    private Dictionary<int, DanceAction> actionsOnBeat;

    public DancerRole(string id)
    {
        this.id = id;
        actionsOnBeat = new Dictionary<int, DanceAction>();
    }

    public void SetGroup(DancerGroup group)
    {
        this.group = group;
        key = GetRoleKey(group.id, id);
    }

    public void AddAction(int beatIndex, DanceAction danceAction)
    {
        actionsOnBeat.Add(beatIndex, danceAction);
    }

    public bool TryGetAction(int beatIndex, out DanceAction danceAction)
    {
        return actionsOnBeat.TryGetValue(beatIndex, out danceAction);
    }

    public static string GetRoleKey(string group, string role)
    {
        return $"{group}-{role}";
    }
}
