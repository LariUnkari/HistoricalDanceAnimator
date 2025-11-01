using UnityEngine;

[CreateAssetMenu(fileName = "New DancePreset", menuName = "ScriptableObjects/DancePreset", order = 1)]
public class DancePreset : ScriptableObject
{
    [System.Serializable]
    public class Part
    {
        public float time;
        public string animatorStateName;
    }

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
    /// <summary>
    /// Points in time and animator state names defining
    /// different parts of the song and dance choreography
    /// </summary>
    public Part[] danceParts;
}
