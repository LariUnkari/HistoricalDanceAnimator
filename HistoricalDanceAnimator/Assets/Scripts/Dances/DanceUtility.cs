using UnityEngine;

public class DanceUtility
{
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
            default: break;
        }

        return Quaternion.identity;
    }
}
