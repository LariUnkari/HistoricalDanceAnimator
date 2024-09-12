[System.Serializable]
public class DanceSet
{
    public string form;
    public string pattern;
    public string sizeType;
    public int sizeCount;
    public int sizeMin;
    public int sizeMax;
    public float separation;
    public string minorType;
    public string[] minorGroups;

    public DanceSet(string form, string pattern,
        string sizeType, int sizeCount, int sizeMin, int sizeMax, float separation,
        string minorType, string[] minorGroups)
    {
        this.form = form;
        this.pattern = pattern;
        this.sizeType = sizeType;
        this.sizeCount = sizeCount;
        this.sizeMin = sizeMin;
        this.sizeMax = sizeMax;
        this.separation = separation;
        this.minorType = minorType;
        this.minorGroups = minorGroups;
    }
}
