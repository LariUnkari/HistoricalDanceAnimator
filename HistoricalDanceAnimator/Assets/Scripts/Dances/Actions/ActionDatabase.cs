using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDatabase
{
    private Dictionary<string, Action> actionsDictionary;

    public void AddAction(string name, string variant, AnimationClip animationClip)
    {
        Action action = new Action(name, variant, animationClip);
        actionsDictionary.Add(action.key, action);
    }

    public bool TryGetAction(string key, out Action action)
    {
        return actionsDictionary.TryGetValue(key, out action);
    }
}
