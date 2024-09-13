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

        DancerRole inactiveRole;
        DancerGroup inactiveGroup;

        foreach (DancerGroup group in groups)
        {
            groupDictionary.Add(group.id, group);

            inactiveGroup = new DancerGroup($"{DancerGroup.INACTIVE_ID}-{group.id}");
            groupDictionary.Add(inactiveGroup.id, inactiveGroup);

            Debug.Log($"Added group with id '{group.id}' and it's inactive counterpart '{inactiveGroup.id}'!");

            foreach (DancerRole role in group.roles)
            {
                if (!roleDictionary.ContainsKey(role.key))
                {
                    roleDictionary.Add(role.key, role);

                    inactiveRole = new DancerRole(role.id, "", 0f);
                    inactiveRole.SetGroup(inactiveGroup);
                    roleDictionary.Add(inactiveRole.key, inactiveRole);

                    Debug.Log($"Added role with key '{role.key}' and it's inactive counterpart '{inactiveRole.key}'!");
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
