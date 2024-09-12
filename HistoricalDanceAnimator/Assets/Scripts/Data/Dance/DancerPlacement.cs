using UnityEngine;

[System.Serializable]
public class DancerPlacement
{
    public string role;
    public string group;
    public Vector2 position;
    public DanceDirection startFacing;

    public DancerPlacement(string role, string group, string variant, Vector2 position, DanceDirection startFacing)
    {
        this.role = role;
        this.group = group;
        this.position = position;
        this.startFacing = startFacing;
    }
}
