using System.Collections.Generic;

[System.Serializable]
public class DanceSet
{
    public DanceSetForm form;
    public string pattern;
    public string sizeType;
    public int sizeCount;
    public int sizeMin;
    public int sizeMax;
    public float separation;
    public string minorType;
    public string[] minorGroups;

    private Dictionary<string, int> minorSetIndices;

    public DanceSet(DanceSetForm form,
        string sizeType, int sizeCount, int sizeMin, int sizeMax, float separation,
        string minorType, string[] minorGroups)
    {
        this.form = form;
        this.sizeType = sizeType;
        this.sizeCount = sizeCount;
        this.sizeMin = sizeMin;
        this.sizeMax = sizeMax;
        this.separation = separation;
        this.minorType = minorType;
        this.minorGroups = minorGroups;

        minorSetIndices = new Dictionary<string, int>();

        for (int i = 0; i < minorGroups.Length; i++)
            minorSetIndices.Add(minorGroups[i], i);
    }

    public int GetMinorSetLength() {
        if (minorGroups != null)
            return minorGroups.Length;

        return 0;
    }

    public int GetGroupMinorIndex(string group)
    {
        if (minorSetIndices.TryGetValue(group, out int index))
            return index;

        return -1;
    }
}
