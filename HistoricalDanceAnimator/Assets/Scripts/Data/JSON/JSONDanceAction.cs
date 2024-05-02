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
    public string startFacing;
    public JSONDanceMovement[] movements;
    public JSONDanceActionTransition transition;
}
