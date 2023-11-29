[System.Serializable]
public class DancerRole
{
    public string id;

    [System.NonSerialized]
    public string key;

    [System.NonSerialized]
    public DancerGroup group;

    public DancerRole(string id)
    {
        this.id = id;
    }

    public void SetGroup(DancerGroup group)
    {
        this.group = group;
        key = GetRoleKey(group.id, id);
    }

    public static string GetRoleKey(string group, string role)
    {
        return $"{group}-{role}";
    }
}
