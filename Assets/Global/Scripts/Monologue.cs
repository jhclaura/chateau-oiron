using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monologue : MonoBehaviour
{
    public AudioClip audioClip;
    public bool updatePosition = true;
    private bool isPlayed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        Debug.Log("player entered!");

        if (isPlayed && MonologueManager.Instance.monoluge.TargetAudio.clip == audioClip)
        {
            MonologueManager.Instance.Resume();
        }
        else
        {
            MonologueManager.Instance.Play(this);
            isPlayed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPlayed && MonologueManager.Instance.monoluge.TargetAudio.clip==audioClip)
        {
            MonologueManager.Instance.Pause();
        }
    }
}
