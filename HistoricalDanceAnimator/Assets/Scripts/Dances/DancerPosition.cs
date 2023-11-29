using UnityEngine;

[System.Serializable]
public class DancerPosition
{
    public string role;
    public string group;
    public string variant;
    public Vector2 position;

    public DancerPosition(string role, string group, string variant, Vector2 position)
    {
        this.role = role;
        this.group = group;
        this.variant = variant;
        this.position = position;
    }
}
