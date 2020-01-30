using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonkeyInForest : MonoBehaviour
{
    public Mascot mascot;
    public Monologue monologue;
    public bool hideAtStart;

    private void Awake()
    {
        if (hideAtStart)
        {
            mascot.Hide();
            mascot.mascotAnimator.enabled = false;
        }
    }

    void Start()
    {
        monologue.TriggerIsEntered += HandleTriggerShowIsEntered;
        monologue.TriggerIsExited += HandleTriggerHideIsEntered;
    }

    void HandleTriggerShowIsEntered()
    {
        if (hideAtStart)
            mascot.mascotAnimator.enabled = true;
        mascot.FadeIn();
    }

    void HandleTriggerHideIsEntered()
    {
        if (hideAtStart)
            mascot.FadeOut();
        else
            mascot.FadeOut(false);
    }
}
