using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MascotForest : MonoBehaviour
{
    public Mascot mascot;
    public bool useAnimatorTrigger;
    public bool useAnimatorFloat;

    [ShowIf("useAnimatorTrigger")]
    public string triggerName;

    [ShowIf("useAnimatorFloat")]
    public string floatName;
    [ShowIf("useAnimatorFloat")]
    public float startFloat;

    private bool isPlayed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || isPlayed)
            return;

        isPlayed = true;

        if(useAnimatorTrigger)
            mascot.TriggerAnimation(triggerName);
        else
            mascot.EnableAnimation();

        if(useAnimatorFloat)
            mascot.mascotAnimator.SetFloat(floatName, startFloat);
    }
}
