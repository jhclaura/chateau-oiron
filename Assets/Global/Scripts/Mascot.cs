using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Mascot : MonoBehaviour
{
    public Animator mascotAnimator;
    public AudioSource mascotAudioSource;

    public bool fadeUseSprite;
    public bool fadeUseMaterial;

    [ShowIf("fadeUseSprite")]
    public SpriteRenderer[] spriteRenderers;
    [ShowIf("fadeUseMaterial")]
    public Material material;

    public void EnableAnimation()
    {
        mascotAnimator.enabled = true;
        mascotAudioSource.Play();
    }

    public void TriggerAnimation(string triggerName)
    {
        mascotAnimator.SetTrigger(triggerName);
        mascotAudioSource.Play();
    }

    public void MoveLeg(float transitionTime=0.2f)
    {
        LeanTween.value(gameObject, 0f, 1f, transitionTime)
            .setOnUpdate((float val) =>
            {
                mascotAnimator.SetFloat("LegMove", val);
            });
    }

    public void StopLeg(float transitionTime=0.2f)
    {
        LeanTween.value(gameObject, 1f, 0f, transitionTime)
            .setOnUpdate((float val) =>
            {
                mascotAnimator.SetFloat("LegMove", val);
            });
    }

    public void FadeIn()
    {
        Debug.Log("mascot fade in");
        LeanTween.cancel(gameObject);
        if (fadeUseSprite)
        {
            Color targetColor = spriteRenderers[0].color;
            targetColor.a = 1f;

            LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                foreach (SpriteRenderer s in spriteRenderers)
                {
                    s.color = Color.Lerp(s.color, targetColor, val);
                }
            });
        }
        else if (fadeUseMaterial)
        {
            Color targetColor = material.color;
            targetColor.a = 1f;
            LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                material.color = Color.Lerp(material.color, targetColor, val);
            });
        }
    }

    public void FadeOut(bool disableAnimation=true)
    {
        Debug.Log("mascot fade out");

        LeanTween.cancel(gameObject);
        if (fadeUseSprite)
        {
            Color targetColor = spriteRenderers[0].color;
            targetColor.a = 0f;

            LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                foreach (SpriteRenderer s in spriteRenderers)
                {
                    s.color = Color.Lerp(s.color, targetColor, val);
                }
            })
            .setOnComplete(()=> {
                if (disableAnimation)
                    mascotAnimator.enabled = false;
            });
        }
        else if(fadeUseMaterial)
        {
            Color targetColor = material.color;
            targetColor.a = 0f;
            LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                material.color = Color.Lerp(material.color, targetColor, val);
            })
            .setOnComplete(() => {
                if (disableAnimation)
                    mascotAnimator.enabled = false;
            });
        }        
    }

    public void Hide()
    {
        if (fadeUseSprite)
        {
            Color targetColor = spriteRenderers[0].color;
            targetColor.a = 0f;
            foreach (SpriteRenderer s in spriteRenderers)
            {
                s.color = targetColor;
            }
        }
        else if (fadeUseMaterial)
        {
            Color targetColor = material.color;
            targetColor.a = 0f;
            material.color = targetColor;
        }
    }
}
