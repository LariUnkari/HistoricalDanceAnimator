[System.Serializable]
public struct JSONDanceAction
{
    public int time;
    public JSONDancerRole[] dancers;
    public string action;
    public string variant;
    public int duration;
    public string startFacing;
    public string endFacing;
    public JSONDanceMovement[] movements;
}
