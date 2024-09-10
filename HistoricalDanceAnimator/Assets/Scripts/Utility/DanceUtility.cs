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

    public static Vector2 GetDancerPositionInFormation(float groupPosition, float rolePosition, string formationType, string formationPattern)
    {
        // TODO: Implement other formation types
        switch (formationType)
        {
            default:
                return new Vector2(rolePosition, groupPosition);
        }
    }

    public static DanceDirection GetDancerFacingDirectionInFormation(float groupPosition, float rolePosition, string orientation, string formationType, string formationPattern)
    {
        // TODO: Implement other formation types
        switch (formationType)
        {
            default:
                return ParseDirection(orientation);
        }
    }

    public static DanceDirection ParseDirection(string direction)
    {
        switch (direction.ToLower())
        {
            case "up":
            case "uphall":
            case "forward":
                return DanceDirection.Up;
            case "down":
            case "downhall":
            case "backward":
                return DanceDirection.Down;
            case "left":
                return DanceDirection.Left;
            case "right":
                return DanceDirection.Right;
            case "upleft":
            case "forwardleft":
                return DanceDirection.UpLeft;
            case "upright":
            case "forwardright":
                return DanceDirection.UpRight;
            case "downright":
            case "backwardright":
                return DanceDirection.DownRight;
            case "downleft":
            case "backwardleft":
                return DanceDirection.DownLeft;
            case "cw":
            case "clockwise":
                return DanceDirection.CW;
            case "ccw":
            case "counterclockwise":
                return DanceDirection.CCW;
            default:
                return DanceDirection.Up;
        }
    }

    public static DanceProgression ParseProgression(string progression)
    {
        switch (progression.ToLower())
        {
            case "line-ab":
                return DanceProgression.Line_AB;
            default:
                return DanceProgression.None;
        }
    }

    public static string GetOrdinalNumberString(int number)
    {
        switch (number)
        {
            case 1: return $"{number}st";
            case 2: return $"{number}nd";
            default: return $"{number}th";
        }
    }
}
