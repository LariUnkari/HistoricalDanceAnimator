using UnityEngine;

[System.Serializable]
public class DanceAction
{
    public string actionName;
    public string variantName;

    public int time;
    public int duration;

    public DanceDirection startFacing;
    public DanceDirection endFacing;

    public DanceMovement movement;

    public AnimationClip animationClip;
    public float animationDuration;

    public string key;

    public DanceAction(string actionName, string variantName, int time, int duration,
        DanceDirection startFacing, DanceDirection endFacing, DanceMovement movement,
        AnimationClip animationClip, float animationDuration)
    {
        this.actionName = actionName;
        this.variantName = variantName;
        this.time = time;
        this.duration = duration;
        this.startFacing = startFacing;
        this.endFacing = endFacing;
        this.movement = movement;
        this.animationClip = animationClip;
        this.animationDuration = animationDuration;

        key = GetActionKey(actionName, variantName);
    }

    public string GetKey()
    {
        return GetActionKey(this.actionName, this.variantName);
    }

    public static string GetActionKey(string name, string variant)
    {
        return string.Format("{0}-{1}", name, variant);
    }
}
