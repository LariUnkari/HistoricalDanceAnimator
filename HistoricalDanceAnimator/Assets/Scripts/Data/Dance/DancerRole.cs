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
    private Dictionary<string, DanceAction> actionsPerId;

    public DancerRole(string id)
    {
        this.id = id;
        actionsOnBeat = new Dictionary<int, DanceAction>();
        actionsPerId = new Dictionary<string, DanceAction>();
    }

    public void SetGroup(DancerGroup group)
    {
        this.group = group;
        key = GetRoleKey(group.id, id);
    }

    public ICollection<DanceAction> GetActionCollection()
    {
        return actionsPerId.Values;
    }

    public void AddAction(int beatIndex, DanceAction danceAction)
    {
        actionsOnBeat.Add(beatIndex, danceAction);

        if (!actionsPerId.ContainsKey(danceAction.GetKey()))
            actionsPerId.Add(danceAction.GetKey(), danceAction);
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
