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
        if (isPlayed) return;

        if (other.gameObject.CompareTag("Player"))
        {
            MonologueManager.Instance.PlayNewMonoluge(this);
            isPlayed = true;
        }
    }
}
