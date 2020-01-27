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
}
