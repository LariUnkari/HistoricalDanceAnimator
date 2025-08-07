using System.Collections;
using UnityEngine;

public class DanceAnimation : MonoBehaviour
{
    public DancePreset m_dancePreset;

    public Animator[] m_animators;
    public AudioSource m_audioSource;
    
	void Start()
    {
        if (m_animators.Length > 0)
        {
            foreach (Animator anim in m_animators)
                anim.enabled = false;
        }

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

        if (m_animators.Length > 0)
        {
            foreach (Animator anim in m_animators)
                StartAnimator(anim);
        }

        if (m_audioSource != null)
        {
            m_audioSource.clip = m_dancePreset.songAudioClip;

            if (m_dancePreset.silenceInBeginning > 0f)
                m_audioSource.time = m_dancePreset.silenceInBeginning;

            m_audioSource.Play();
        }
    }

    private void StartAnimator(Animator anim)
    {
        anim.runtimeAnimatorController = m_dancePreset.animatorController;
        anim.speed = m_dancePreset.songBPM / (60f * (m_dancePreset.animationBPS > 0f ? m_dancePreset.animationBPS : 1f));
        anim.enabled = true;
    }
}
