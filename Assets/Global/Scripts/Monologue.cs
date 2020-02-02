using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monologue : MonoBehaviour
{
    public AudioClip audioClip;
    public bool updatePosition = true;
    private bool isPlayed;
    private bool isFinished;

    public System.Action TriggerIsEntered;
    public System.Action TriggerIsExited;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        Debug.Log("player entered!");
        TriggerIsEntered?.Invoke();

        if (isFinished)
            return;

        if (isPlayed && MonologueManager.Instance.monolouge.TargetAudio.clip == audioClip)
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
        if (isPlayed && MonologueManager.Instance.monolouge.TargetAudio.clip==audioClip)
        {
            MonologueManager.Instance.Pause();
            Debug.Log("player exited!");
            TriggerIsExited?.Invoke();
        }
    }

    public void Finished()
    {
        isFinished = true;
    }
}
