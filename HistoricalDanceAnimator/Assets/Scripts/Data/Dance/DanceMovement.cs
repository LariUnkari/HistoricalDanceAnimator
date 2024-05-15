using UnityEngine;

[System.Serializable]
public class DanceMovement
{
    public DanceVector[] directions;
    public Vector3 vector;
    public Vector3 cross;

    public DanceMovement(DanceVector[] directions, Vector3 vector)
    {
        this.directions = directions;
        this.vector = vector;
        this.cross = Vector3.Cross(vector, Vector3.forward);
    }

    public override string ToString()
    {
        return this.directions != null ? string.Join(",", this.directions) : "*None*";
    }
}
