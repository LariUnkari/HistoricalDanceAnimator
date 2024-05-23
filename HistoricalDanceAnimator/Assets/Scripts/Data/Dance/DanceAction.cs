using UnityEngine;

[System.Serializable]
public class DanceAction
{
    public string familyName;
    public string actionName;
    public string variantName;

    public string part;
    public int time;
    public int duration;

    public DanceDirection startFacing;

    public DanceMovement movement;

    public DanceActionTransitions transitions;

    public AnimationClip animationClip;
    public float animationDuration;

    public string key;

    public DanceAction(string familyName, string actionName, string variantName, string part, int time, int duration,
        DanceDirection startFacing, DanceMovement movement, DanceActionTransitions transitions, AnimationClip animationClip, float animationDuration)
    {
        this.familyName = familyName;
        this.actionName = actionName;
        this.variantName = variantName;
        this.part = part;
        this.time = time;
        this.duration = duration;
        this.startFacing = startFacing;
        this.movement = movement;
        this.transitions = transitions;
        this.animationClip = animationClip;
        this.animationDuration = animationDuration;

        key = GetActionKey(familyName, actionName, variantName);
    }

    public static string GetActionKey(string family, string name, string variant)
    {
        string key = name;

        if (variant.Length > 0) key = $"{key}-{variant}";
        if (family.Length > 0) key = $"{family}-{key}";

        return key;
    }
}
