using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ActionPreset", menuName = "ScriptableObject/Actions/ActionPreset")]
public class ActionPreset : ScriptableObject
{
    public string action;
    public string variant;
    public float duration = 1f;
    public AnimationClip animation;
}
