using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DancePreset")]
public class DancePreset : ScriptableObject
{
    public AudioClip songAudioClip;
    public float songBPM = 100f;
    public float silenceInBeginning;
    public RuntimeAnimatorController animatorController;
    public float animationBPS = 4f;
}
