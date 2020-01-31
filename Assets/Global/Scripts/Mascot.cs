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

    public bool pauseAudioWhenLegsStop;
    private AnimatedAudio animatedAudio;
    private int legTweenId = -1;
    private int fadeTweenId = -1;

    private void Start()
    {
        animatedAudio = GetComponent<AnimatedAudio>();
    }

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
        LeanTween.value(0f, 1f, transitionTime)
            .setOnUpdate((float val) =>
            {
                mascotAnimator.SetFloat("LegMove", val);
            });

        if (pauseAudioWhenLegsStop) animatedAudio.Toggle(true, 1f, .2f, 0f);
    }

    public void StopLeg(float transitionTime=0.2f)
    {
        LeanTween.value(1f, 0f, transitionTime)
            .setOnUpdate((float val) =>
            {
                mascotAnimator.SetFloat("LegMove", val);
            });

        if (pauseAudioWhenLegsStop) animatedAudio.Toggle(false, 0f, .2f, 0f);
    }

    public void FadeIn()
    {
        Debug.Log("mascot fade in");
        if (LeanTween.isTweening(fadeTweenId))
        {
            LeanTween.cancel(fadeTweenId);
            fadeTweenId = -1;
        }

        if (fadeUseSprite)
        {
            Color targetColor = spriteRenderers[0].color;
            targetColor.a = 1f;

            fadeTweenId =  LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                foreach (SpriteRenderer s in spriteRenderers)
                {
                    s.color = Color.Lerp(s.color, targetColor, val);
                }
            }).id;
        }
        else if (fadeUseMaterial)
        {
            Color targetColor = material.color;
            targetColor.a = 1f;
            fadeTweenId = LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                material.color = Color.Lerp(material.color, targetColor, val);
            }).id;
        }
    }

    public void FadeOut(bool disableAnimation=true)
    {
        Debug.Log("mascot fade out");

        if (LeanTween.isTweening(fadeTweenId))
        {
            LeanTween.cancel(fadeTweenId);
            fadeTweenId = -1;
        }

        if (fadeUseSprite)
        {
            Color targetColor = spriteRenderers[0].color;
            targetColor.a = 0f;

            fadeTweenId = LeanTween.value(gameObject, 0f, 1f, 2f)
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
            }).id;
        }
        else if(fadeUseMaterial)
        {
            Color targetColor = material.color;
            targetColor.a = 0f;
            fadeTweenId = LeanTween.value(gameObject, 0f, 1f, 2f)
            .setOnUpdate((float val) =>
            {
                material.color = Color.Lerp(material.color, targetColor, val);
            })
            .setOnComplete(() => {
                if (disableAnimation)
                    mascotAnimator.enabled = false;
            }).id;
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
