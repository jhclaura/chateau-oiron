using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotFire : MonoBehaviour
{
    public Monologue triggerShowMonologue;
    public Monologue triggerHideMonologue;
    public SpriteRenderer[] spriteRenderers;

    private Animator animator;

    void Start()
    {
        triggerShowMonologue.TriggerIsEntered += HandleTriggerShowIsEntered;
        triggerHideMonologue.TriggerIsEntered += HandleTriggerHideIsEntered;
        animator = GetComponent<Animator>();
    }

    void HandleTriggerShowIsEntered()
    {
        animator.enabled = true;
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
            })
            .setOnComplete(()=>
            {
                animator.enabled = false;
            });
    }
}
