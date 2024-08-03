using UnityEngine;

public class DanceUtility
{
    public static Vector3 Vector3UpLeft = new Vector3(-1f, 1f, 0f).normalized;
    public static Vector3 Vector3UpRight = new Vector3(1f, 1f, 0f).normalized;
    public static Vector3 Vector3DownRight = new Vector3(1f, -1f, 0f).normalized;
    public static Vector3 Vector3DownLeft = new Vector3(-1f, -1f, 0f).normalized;

    public static Vector3 OrientateVector(Vector3 localVector, DanceVector danceVector)
    {
        return GetRotationFromDirection(danceVector.direction) * localVector * danceVector.distance;
    }

    public static Vector3 GetVectorFromDirection(DanceDirection danceDirection)
    {
        switch (danceDirection)
        {
            case DanceDirection.Down:  return Vector3.down;
            case DanceDirection.Left:  return Vector3.left;
            case DanceDirection.Right: return Vector3.right;
            case DanceDirection.UpLeft: return Vector3UpLeft;
            case DanceDirection.UpRight: return Vector3UpRight;
            case DanceDirection.DownRight: return Vector3DownRight;
            case DanceDirection.DownLeft: return Vector3DownLeft;
            default: break;
        }

        return Vector3.up;
    }

    public static Quaternion GetRotationFromDirection(DanceDirection danceDirection)
    {
        switch (danceDirection)
        {
            case DanceDirection.Down:  return Quaternion.AngleAxis(180, Vector3.forward);
            case DanceDirection.Left:  return Quaternion.AngleAxis(90, Vector3.forward);
            case DanceDirection.Right: return Quaternion.AngleAxis(-90, Vector3.forward);
            case DanceDirection.UpLeft: return Quaternion.AngleAxis(45, Vector3.forward);
            case DanceDirection.UpRight: return Quaternion.AngleAxis(-45, Vector3.forward);
            case DanceDirection.DownRight: return Quaternion.AngleAxis(-135, Vector3.forward);
            case DanceDirection.DownLeft: return Quaternion.AngleAxis(135, Vector3.forward);
            default: break;
        }

        return Quaternion.identity;
    }
}
