using UnityEngine;

[System.Serializable]
public struct Action
{
    public string actionName;
    public string variantName;
    public AnimationClip animationClip;
    public string key;

    public Action(string actionName, string variantName, AnimationClip animationClip)
    {
        this.actionName = actionName;
        this.variantName = variantName;
        this.animationClip = animationClip;

        key = GetActionKey(actionName, variantName);
    }

    public static string GetActionKey(string name, string variant)
    {
        return string.Format("{0}-{1}", name, variant);
    }
}
