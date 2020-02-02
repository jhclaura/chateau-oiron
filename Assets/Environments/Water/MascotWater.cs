using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotWater : MonoBehaviour
{
    public Mascot mascot;
    public Monologue targetMonologue;

    private bool isDisappeared;

    private void OnEnable()
    {
        EventBus.MonologueEnded.AddListener(HandleMonologueEnded);
    }

    private void OnDisable()
    {
        EventBus.MonologueEnded.RemoveListener(HandleMonologueEnded);
    }

    private void HandleMonologueEnded()
    {
        if (MonologueManager.Instance.monolouge.TargetAudio.clip == targetMonologue.audioClip)
        {
            if (!isDisappeared)
            {
                isDisappeared = true;
                mascot.FadeOut();
            }
        }
    }
}
