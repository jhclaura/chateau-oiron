using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonologueManager : Manager<MonologueManager>
{
    public AnimatedAudio monoluge;
    public UnityEngine.Audio.AudioMixerSnapshot normalSnapShot;
    public UnityEngine.Audio.AudioMixerSnapshot monologueSnapShot;

    private WaitForSeconds fadeDelay;

    void Start()
    {
        fadeDelay = new WaitForSeconds(0.55f);
    }

    public void Play(Monologue newMonologue)
    {
        StartCoroutine(FadeIn(newMonologue));
        monologueSnapShot.TransitionTo(1.5f);
    }

    IEnumerator FadeIn(Monologue newMonologue)
    {
        if (monoluge.IsPlaying)
        {
            monoluge.Toggle(false, 0, .5f, 0f, true);
            yield return fadeDelay;
        }
        monoluge.TargetAudio.clip = newMonologue.audioClip;
        monoluge.transform.position = newMonologue.transform.position;
        monoluge.Toggle(true, 1, 1f, 0f);
    }

    public void Pause()
    {
        if (monoluge.IsPlaying)
        {
            monoluge.Toggle(false, 0, 1f, 0f);
            normalSnapShot.TransitionTo(1.5f);
        }
    }

    public void Resume()
    {
        if (!monoluge.IsPlaying)
        {
            monoluge.Toggle(true, monoluge.OriginalVolumn, .5f, 0f);
            monologueSnapShot.TransitionTo(1f);
        }
    }
}
