using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PawnModelDatabase", menuName = "ScriptableObject/Pawn/PawnModelDatabase")]
public class PawnModelDatabase : ScriptableObject
{
    private static PawnModelDatabase s_instance;

    public static PawnModelDatabase GetInstance()
    {
        return s_instance;
    }

    public static string GetPresetKey(string role, string group, string variant)
    {
        return $"{role}{group}{variant}";
    }

    public PawnModelPreset[] presets;

    private Dictionary<string, PawnModelPreset> presetDictionary;

    public void Init()
    {
        if (s_instance)
            return;

        s_instance = this;
        presetDictionary = new Dictionary<string, PawnModelPreset>();

        foreach (PawnModelPreset preset in presets)
        {
            presetDictionary.Add(GetPresetKey(preset.role, preset.group, preset.variant), preset);
        }
    }

    public PawnModelPreset GetPreset(string key)
    {
        return presetDictionary[key];
    }
}
