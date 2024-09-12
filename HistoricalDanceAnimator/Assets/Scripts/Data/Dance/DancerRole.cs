using System.Collections.Generic;

[System.Serializable]
public class DancerRole
{
    [System.NonSerialized]
    public DancerGroup group;

    public string id;
    public string key;
    public float renderOffset;

    private string variant;
    private Dictionary<int, DanceAction> actionsOnBeat;
    private Dictionary<string, DanceAction> actionsPerId;

    public string Variant { get { return variant; } }

    public DancerRole(string id, string variant, float renderOffset)
    {
        this.id = id;
        this.variant = variant;
        this.renderOffset = renderOffset;
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

        if (!actionsPerId.ContainsKey(danceAction.key))
            actionsPerId.Add(danceAction.key, danceAction);
    }

    public bool TryGetAction(int beatIndex, out DanceAction danceAction)
    {
        return actionsOnBeat.TryGetValue(beatIndex, out danceAction);
    }

    public void ResetActionTransitions()
    {
        foreach (DanceAction action in actionsOnBeat.Values)
        {
            if (action.transitions != null)
                action.transitions.Reset();
        }
    }

    public static string GetRoleKey(string group, string role)
    {
        return $"{group}-{role}";
    }
}
