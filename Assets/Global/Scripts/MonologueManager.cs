using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonologueManager : Manager<MonologueManager>
{
    public AnimatedAudio monolouge;
    public UnityEngine.Audio.AudioMixerSnapshot normalSnapShot;
    public UnityEngine.Audio.AudioMixerSnapshot monologueSnapShot;

    private WaitForSeconds fadeDelay;
    private float currentMonologueLength;
    private float currentMonologueStartTimestamp;
    private bool currentMonologueIsPlaying;
    private Monologue currentMonologue;

    void Start()
    {
        fadeDelay = new WaitForSeconds(0.55f);
    }

    private void OnEnable()
    {
        EventBus.EnteredTranitionStartTrigger.AddListener(HandleEnteredTransitionStartTrigger);
    }

    private void OnDisable()
    {
        EventBus.EnteredTranitionStartTrigger.RemoveListener(HandleEnteredTransitionStartTrigger);

    }

    private void Update()
    {
        if (currentMonologueIsPlaying && (Time.time- currentMonologueStartTimestamp>currentMonologueLength))
        {
            currentMonologueIsPlaying = false;
            currentMonologue.Finished();
            EventBus.MonologueEnded.Invoke();
            normalSnapShot.TransitionTo(1.5f);
        }
    }

    public void Play(Monologue newMonologue)
    {
        StartCoroutine(FadeIn(newMonologue));
        monologueSnapShot.TransitionTo(1.5f);
    }

    IEnumerator FadeIn(Monologue newMonologue)
    {
        if (monolouge.IsPlaying)
        {
            monolouge.Toggle(false, 0, .5f, 0f, true);
            currentMonologueIsPlaying = false;
            yield return fadeDelay;
        }
        Debug.Log("play monologue");
        monolouge.TargetAudio.clip = newMonologue.audioClip;
        monolouge.transform.position = newMonologue.transform.position;

        currentMonologueIsPlaying = true;
        currentMonologueStartTimestamp = Time.time;
        currentMonologueLength = newMonologue.audioClip.length;
        currentMonologue = newMonologue;

        monolouge.Toggle(true, 1, 1f, 0f);
    }

    public void Pause()
    {
        Debug.Log("pause monologue");
        if (monolouge.IsPlaying)
        {
            monolouge.Toggle(false, 0, 1.5f, 0f);
            normalSnapShot.TransitionTo(2f);

            currentMonologueIsPlaying = false;
            currentMonologueStartTimestamp += 1.5f;
        }
    }

    public void Resume()
    {
        Debug.Log("resume monologue");
        if (!monolouge.IsPlaying)
        {
            monolouge.Toggle(true, monolouge.OriginalVolumn, 1f, 0f);
            monologueSnapShot.TransitionTo(1.5f);

            currentMonologueIsPlaying = true;
        }
    }

    void HandleEnteredTransitionStartTrigger(EnvironmentType toEnv)
    {
        // stop monologue
        monolouge.StopWithoutCheck();
    }
}
