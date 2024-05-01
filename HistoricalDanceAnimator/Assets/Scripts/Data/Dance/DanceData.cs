using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanceData
{
    public string danceName;
    public string danceType;
    public string danceFamily;

    public AudioClip music;
    public float bpm;
    public float firstBeatTime;

    public DancerGroup[] groups;
    public DancerPlacement[] placements;
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

    public bool TryGetGroup(string groupID, out DancerGroup dancerGroup)
    {
        return groupDictionary.TryGetValue(groupID, out dancerGroup);
    }

    public bool TryGetRole(string key, out DancerRole dancerRole)
    {
        return roleDictionary.TryGetValue(key, out dancerRole);
    }
}
