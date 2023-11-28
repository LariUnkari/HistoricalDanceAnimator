using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DancePreset")]
public class DancePreset : ScriptableObject
{
    [Tooltip("Audio clip of the music")]
    /// <summary>
    /// Audio clip of the music
    /// </summary>
    public AudioClip songAudioClip;
    /// <summary>
    /// Beats per minute in the music clip
    /// </summary>
    public float songBPM = 100f;
    /// <summary>
    /// Silence in the begining of music clip, in seconds
    /// </summary>
    public float silenceInBeginning;
    /// <summary>
    /// Animator controller that defines all the animations
    /// </summary>
    public RuntimeAnimatorController animatorController;
    /// <summary>
    /// Animation speed, beats per second of animation
    /// </summary>
    public float animationBPS = 4f;
}
