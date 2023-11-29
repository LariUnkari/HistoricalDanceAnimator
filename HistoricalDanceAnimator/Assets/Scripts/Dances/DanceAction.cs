using UnityEngine;

[System.Serializable]
public class DanceAction
{
    public string actionName;
    public string variantName;

    public int duration;

    public DanceDirection startFacing;
    public DanceDirection endFacing;

    public DanceMovement movement;

    public AnimationClip animationClip;

    public string key;

    public DanceAction(string actionName, string variantName, int duration, DanceDirection startFacing,
        DanceDirection endFacing, DanceMovement movement, AnimationClip animationClip)
    {
        this.actionName = actionName;
        this.variantName = variantName;
        this.duration = duration;
        this.startFacing = startFacing;
        this.endFacing = endFacing;
        this.movement = movement;
        this.animationClip = animationClip;

        key = GetActionKey(actionName, variantName);
    }

    public static string GetActionKey(string name, string variant)
    {
        return string.Format("{0}-{1}", name, variant);
    }
}
