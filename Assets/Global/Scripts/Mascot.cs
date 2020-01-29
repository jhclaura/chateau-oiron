using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mascot : MonoBehaviour
{
    public Animator mascotAnimator;
    public AudioSource mascotAudioSource;

    public void EnableAnimation()
    {
        mascotAnimator.enabled = true;
        mascotAudioSource.Play();
    }

    public void TriggerAnimation(string triggerName)
    {
        mascotAnimator.SetTrigger(triggerName);
        mascotAudioSource.Play();
    }

    public void MoveLeg(float transitionTime=0.2f)
    {
        LeanTween.value(gameObject, 0f, 1f, transitionTime)
            .setOnUpdate((float val) =>
            {
                mascotAnimator.SetFloat("LegMove", val);
            });
    }

    public void StopLeg(float transitionTime=0.2f)
    {
        LeanTween.value(gameObject, 1f, 0f, transitionTime)
            .setOnUpdate((float val) =>
            {
                mascotAnimator.SetFloat("LegMove", val);
            });
    }
}
