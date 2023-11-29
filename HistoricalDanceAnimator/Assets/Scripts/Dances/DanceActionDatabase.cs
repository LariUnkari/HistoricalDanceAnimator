using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceActionDatabase
{
    private Dictionary<string, DanceAction> actionsDictionary;

    public void AddAction(DanceAction action)
    {
        actionsDictionary.Add(action.key, action);
    }

    public bool TryGetAction(string key, out DanceAction action)
    {
        return actionsDictionary.TryGetValue(key, out action);
    }
}
