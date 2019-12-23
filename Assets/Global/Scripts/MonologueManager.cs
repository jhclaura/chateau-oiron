using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonologueManager : Manager<MonologueManager>
{
    public AnimatedAudio monoluge;

    private WaitForSeconds fadeDelay;

    void Start()
    {
        fadeDelay = new WaitForSeconds(0.55f);
    }

    public void PlayNewMonoluge(Monologue newMonologue)
    {
        StartCoroutine(FadeOutFadeInMonolgue(newMonologue));
    }

    IEnumerator FadeOutFadeInMonolgue(Monologue newMonologue)
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
}
