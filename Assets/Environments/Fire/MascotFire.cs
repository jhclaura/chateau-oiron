using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotFire : MonoBehaviour
{
    public Mascot mascot;
    public Monologue triggerShowMonologue;
    public Monologue triggerHideMonologue;
    public SpriteRenderer[] spriteRenderers;

    void Start()
    {
        triggerShowMonologue.TriggerIsEntered += HandleTriggerShowIsEntered;
        triggerHideMonologue.TriggerIsEntered += HandleTriggerHideIsEntered;
    }

    void HandleTriggerShowIsEntered()
    {
        mascot.TriggerAnimation("StartWalking");
        mascot.mascotAnimator.SetFloat("LegMove", 1f);
    }

    void HandleTriggerHideIsEntered()
    {
        Color clearColor = spriteRenderers[0].color;
        clearColor.a = 0f;
        LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val)=>
            {
                foreach(SpriteRenderer s in spriteRenderers)
                {
                    s.color = Color.Lerp(s.color, clearColor, val);
                }
                mascot.mascotAudioSource.volume = 1f - val;
            })
            .setOnComplete(()=>
            {
                mascot.mascotAnimator.enabled = false;
            });
    }
}
