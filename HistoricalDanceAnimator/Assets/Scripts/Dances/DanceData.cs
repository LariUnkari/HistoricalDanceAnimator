using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanceData
{
    public string danceName;
    public string danceType;

    public AudioClip music;
    public int bpm;
    public int firstBeatTime;

    public DancerGroup[] groups;
    public DanceFormation formation;
    public DanceAction[] actions;

    private Dictionary<string, DancerGroup> groupDictionary;
    private Dictionary<string, DancerRole> roleDictionary;

    public void SetGroups(DancerGroup[] groups)
    {
        this.groups = groups;

        groupDictionary = new Dictionary<string, DancerGroup>();
        roleDictionary = new Dictionary<string, DancerRole>();

        foreach (DancerGroup group in groups)
        {
            groupDictionary.Add(group.id, group);

            foreach (DancerRole role in group.roles)
            {
                roleDictionary.Add(role.key, role);
            }
        }
    }

    public DancerGroup GetGroup(string groupID)
    {
        return groupDictionary[groupID];
    }
}
