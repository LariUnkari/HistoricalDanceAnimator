[System.Serializable]
public class DancerGroup
{
    public const string INACTIVE_ID = "Inactive";

    public string id;
    public DancerRole[] roles;

    public DancerGroup(string id)
    {
        this.id = id;
    }

    public void SetRoles(DancerRole[] roles)
    {
        this.roles = roles;
    }
}
