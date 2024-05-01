using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ActionPresetDatabase", menuName = "ScriptableObject/Actions/ActionPresetDatabase")]
public class ActionPresetDatabase : ScriptableObject
{
    private static ActionPresetDatabase s_instance;

    public static ActionPresetDatabase GetInstance()
    {
        return s_instance;
    }

    public ActionPreset idle;
    public ActionPreset[] presets;

    private Dictionary<string, ActionPreset> presetDictionary;

    public void Init()
    {
        if (s_instance)
            return;

        s_instance = this;

        Debug.Log(GetType() + ": Init!");

        presetDictionary = new Dictionary<string, ActionPreset>();
        presetDictionary.Add(GetPresetKey(idle.family, idle.action, idle.variant), idle);

        foreach (ActionPreset preset in presets)
        {
            presetDictionary.Add(GetPresetKey(preset.family, preset.action, preset.variant), preset);
        }
    }

    public static string GetPresetKey(string family, string action, string variant)
    {
        string key = action;

        if (family.Length > 0) key = $"{family}-{key}";
        if (variant.Length > 0) key = $"{key}-{variant}";

        return key;
    }

    public bool TryGetPreset(string key, out ActionPreset actionPreset)
    {
        return presetDictionary.TryGetValue(key, out actionPreset);
    }
}
