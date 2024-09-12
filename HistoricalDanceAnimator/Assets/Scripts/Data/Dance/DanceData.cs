using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanceData
{
    public string danceName;
    public string danceFamily;
    public DanceSet danceSet;
    public DanceProgression danceProgression;
    public int danceRepeats;
    public int danceLength;

    public AudioClip music;
    public MusicBPMChanges bpmChanges;
    public float firstBeatTime;

    public DancerGroup[] groups;
    public DancerPlacement[] placements;
    public DancePart[] choreography;

    private Dictionary<string, DancerGroup> groupDictionary;
    private Dictionary<string, DancerRole> roleDictionary;

    public void SetGroups(DancerGroup[] groups)
    {
        this.groups = groups;

        groupDictionary = new Dictionary<string, DancerGroup>();
        roleDictionary = new Dictionary<string, DancerRole>();

        DancerGroup inactiveGroup = new DancerGroup("inactive");
        DancerRole inactiveLord = new DancerRole("Lord", "", 0f);
        DancerRole inactiveLady = new DancerRole("Lady", "", 0f);

        groupDictionary.Add(inactiveGroup.id, inactiveGroup);
        inactiveLord.SetGroup(inactiveGroup);
        inactiveLady.SetGroup(inactiveGroup);
        roleDictionary.Add(inactiveLord.key, inactiveLord);
        roleDictionary.Add(inactiveLady.key, inactiveLady);

        foreach (DancerGroup group in groups)
        {
            groupDictionary.Add(group.id, group);

            foreach (DancerRole role in group.roles)
            {
                if (!roleDictionary.ContainsKey(role.key))
                {
                    Debug.Log($"Role with key '{role.key}' added!");
                    roleDictionary.Add(role.key, role);
                }
                else
                    Debug.LogError($"Role with key '{role.key}' already exists in dictionary!");
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
