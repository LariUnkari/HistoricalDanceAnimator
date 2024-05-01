using System.Collections.Generic;

public class DanceActionDatabase
{
    public static DanceActionDatabase s_instance;

    public static DanceActionDatabase GetInstance()
    {
        if (s_instance == null)
            s_instance = new DanceActionDatabase();

        return s_instance;
    }

    private Dictionary<string, DanceAction> actionsDictionary;

    public DanceActionDatabase()
    {
        actionsDictionary = new Dictionary<string, DanceAction>();
    }

    public void AddAction(DanceAction action)
    {
        actionsDictionary.Add(action.key, action);
    }

    public bool TryGetAction(string key, out DanceAction action)
    {
        return actionsDictionary.TryGetValue(key, out action);
    }
}
