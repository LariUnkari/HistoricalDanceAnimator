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
            Play(0);

        for (KeyCode kc = KeyCode.Alpha1; kc <= KeyCode.Alpha9; kc++)
        {
            if (Input.GetKeyDown(kc))
            {
                Play(1 + (int)kc - (int)KeyCode.Alpha1);
            }
        }
    }

    public void Play(int partIndex)
    {
        if (m_dancePreset == null)
            return;

        DancePreset.Part part = null;
        if (partIndex > 0)
        {
            partIndex--;

            if (partIndex < m_dancePreset.danceParts.Length)
                part = m_dancePreset.danceParts[partIndex];
        }

        if (m_animators.Length > 0)
        {
            float speed = m_dancePreset.songBPM / (60f * (m_dancePreset.animationBPS > 0f ? m_dancePreset.animationBPS : 1f));
            Debug.Log($"Playing {m_animators.Length} animator(s) at speed {speed:F3}. Timescale is {Time.timeScale:F3}");

            foreach (Animator anim in m_animators)
                StartAnimator(anim, speed, part);
        }

        if (m_audioSource != null)
        {
            m_audioSource.clip = m_dancePreset.songAudioClip;

            if (part != null)
                m_audioSource.time = part.time;
            else if (m_dancePreset.silenceInBeginning > 0f)
                m_audioSource.time = m_dancePreset.silenceInBeginning;

            m_audioSource.Play();
        }
    }

    private void StartAnimator(Animator anim, float speed, DancePreset.Part dancePart)
    {
        if (m_dancePreset.animatorController)
        {
            anim.runtimeAnimatorController = m_dancePreset.animatorController;
        }

        anim.speed = speed;
        anim.enabled = true;

        if (dancePart != null && dancePart.animatorStateName.Length > 0)
        {
            int state = Animator.StringToHash(dancePart.animatorStateName);
            if (anim.HasState(0, state))
                anim.Play(state, 0);
        }
    }
}
