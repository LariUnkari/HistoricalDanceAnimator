[System.Serializable]
public struct JSONDanceAction
{
    public int time;
    public string part;
    public JSONDancerRole[] dancers;
    public string family;
    public string action;
    public string variant;
    public int duration;
    public int repeat;
    public string startFacing;
    public JSONDanceMovement[] movements;
    public JSONDanceActionPositionTransition[] positionTransitions;
    public JSONDanceActionRotationTransition[] rotationTransitions;
}
