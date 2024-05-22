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

        string key;
        foreach (ActionPreset preset in presets)
        {
            key = GetPresetKey(preset.family, preset.action, preset.variant);
            presetDictionary.Add(key, preset);
            Debug.Log($"Added {typeof(ActionPreset)} instance by key '{key}' from '{preset.name}'");
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
