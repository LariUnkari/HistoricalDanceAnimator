using System.Collections;
using UnityEngine;

public class DanceAnimation : MonoBehaviour
{
    public DancePreset m_dancePreset;

    public Animator m_animator;
    public AudioSource m_audioSource;
    
	void Start()
    {
        if (m_animator != null)
            m_animator.enabled = false;

        if (m_audioSource != null && m_audioSource.playOnAwake)
            m_audioSource.Stop();
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Play();
    }

    public void Play()
    {
        if (m_dancePreset == null)
            return;

        if (m_animator != null)
        {
            m_animator.runtimeAnimatorController = m_dancePreset.animatorController;
            m_animator.speed = m_dancePreset.songBPM / (60f * (m_dancePreset.animationBPS > 0f ? m_dancePreset.animationBPS : 1f));
            m_animator.enabled = true;
        }

        if (m_audioSource != null)
        {
            m_audioSource.clip = m_dancePreset.songAudioClip;

            if (m_dancePreset.silenceInBeginning > 0f)
                m_audioSource.time = m_dancePreset.silenceInBeginning;

            m_audioSource.Play();
        }
    }
}
