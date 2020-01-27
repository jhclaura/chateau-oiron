using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotForest : MonoBehaviour
{
    public Mascot mascot;

    private bool isPlayed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || isPlayed)
            return;

        isPlayed = true;
        mascot.EnableAnimation();
    }
}
