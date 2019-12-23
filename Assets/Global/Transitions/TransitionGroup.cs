using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TransitionGroup : MonoBehaviour
{
    [ReorderableList]
    public TransitionScriptableObject[] transitions;
    public bool visibleAnchor;
    [ReorderableList]
    public TransitionAnchor[] transitionAnchors;

    [Space(10)]
    public AnimatedAudio animatedAudio;
    public Monologue diatome;
    public GameObject transitionCube;

    [Space(10)]
    public bool visibleTrigger;
    public TransitionTrigger startTransitionTrigger;
    public TransitionTrigger endTransitionTrigger;
    public GameObject turnGraphic;
    public TMPro.TextMeshPro turnText;
    public TMPro.TextMeshPro frontText;

    public Dictionary<EnvironmentType, TransitionScriptableObject> envToTransitionObject = new Dictionary<EnvironmentType, TransitionScriptableObject>();
    public Dictionary<EnvironmentType, TransitionAnchor> envToTransitionAnchor = new Dictionary<EnvironmentType, TransitionAnchor>();

    private EnvironmentType currentTransitionToEnv;
    private Material transitionCubeMaterial;
    private IEnumerator duringTransitionCoroutine;

    private void OnEnable()
    {
        startTransitionTrigger.OnEnterTrigger += HandleEnterStartTransitionTrigger;
        endTransitionTrigger.OnEnterTrigger += HandleEnterEndTransitionTrigger;
    }

    private void OnDisable()
    {
        startTransitionTrigger.OnEnterTrigger -= HandleEnterStartTransitionTrigger;
        endTransitionTrigger.OnEnterTrigger -= HandleEnterEndTransitionTrigger;
    }

    void Awake()
    {
        foreach (TransitionScriptableObject tran in transitions)
        {
            envToTransitionObject.Add(tran.toEnvironment, tran);
        }
        foreach (TransitionAnchor tran in transitionAnchors)
        {
            envToTransitionAnchor.Add(tran.toEnvironment, tran);
            if (!visibleAnchor)
            {
                tran.gameObject.SetActive(false);
            }
        }

        transitionCubeMaterial = transitionCube.GetComponent<Renderer>().sharedMaterial;
        transitionCubeMaterial.color = Color.black;
        transitionCube.SetActive(true);

        if (!visibleTrigger)
        {
            startTransitionTrigger.GetComponent<MeshRenderer>().enabled = false;
            endTransitionTrigger.GetComponent<MeshRenderer>().enabled = false;
        }

        Reset();
    }

    // Called by ChateauSceneManager
    public void StartTransition(EnvironmentType env, bool skipFade = false)
    {
        currentTransitionToEnv = env;

        animatedAudio.TargetAudio.clip = envToTransitionObject[env].audioClip;
        animatedAudio.ToggleOn();

       

        // Start waiting for transition to end, if not ended by User (by walking out of transition)
        duringTransitionCoroutine = DuringTransition(envToTransitionObject[env].audioClip.length);
        StartCoroutine(duringTransitionCoroutine);

        // Just show Transition Cube (case: Calibration Start)
        if (skipFade)
        {
            EventBus.TransitionStarted.Invoke();
            turnGraphic.SetActive(true);
            turnText.gameObject.SetActive(true);
            Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " starts!");
        }
        else // Fade in Transition Cube
        {
            LTDescr tween = FadeInTransitionCube(envToTransitionObject[env].interiorColor);
            tween.setOnComplete(() => {
                EventBus.TransitionStarted.Invoke();
                if (env==EnvironmentType.Forest)
                {
                    turnGraphic.SetActive(true);
                    turnText.gameObject.SetActive(true);
                }
                else
                {
                    frontText.gameObject.SetActive(true);
                }
                
                Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " starts!");
            });
        }

        if(currentTransitionToEnv == EnvironmentType.End)
        {
            frontText.text = "Closing starts. Proceed to end it.";
            frontText.color = Color.white;
            frontText.gameObject.SetActive(true);
            turnText.text = "";
        }
        else
        {
            if (env == EnvironmentType.Forest)
                turnText.text = "Diatom lost.\nDiatom found.\nYou are now connected to the\nlabyrinth network.\nTurn around and step forward.";
            else if (env != EnvironmentType.Water)
                frontText.text = "Diatom lost.\nDiatom found.\nYou are now connected to the\nlabyrinth network.\nProceed with caution.";

            // diatome lost
            MonologueManager.Instance.PlayNewMonoluge(diatome);
        }
    }

    // Called by ChateauSceneManager
    public void EndTransition(bool doFadeOut=true)
    {
        animatedAudio.Stop(true, 0.5f);
        turnGraphic.SetActive(false);
        turnText.gameObject.SetActive(false);
        frontText.gameObject.SetActive(false);
        if (currentTransitionToEnv == EnvironmentType.End)
        {
            frontText.text = "";
            frontText.gameObject.SetActive(false);
        }

        if (duringTransitionCoroutine!=null)
        {
            StopCoroutine(duringTransitionCoroutine);
            duringTransitionCoroutine = null;
        }

        // Fade out Transition Cube
        if (doFadeOut)
        {
            LTDescr tween = FadeOutTransitionCube();
            tween.setOnComplete(() => {
                transitionCube.SetActive(false);
                EventBus.TransitionEnded.Invoke();
                Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " ends!");
            });
        }
        else
        {
            // fade to black
            LeanTween.value(transitionCube, 0f, 1f, 1f)
                .setOnUpdate((float val) => {
                    transitionCubeMaterial.color = Color.Lerp(transitionCubeMaterial.color, Color.black, val);
                }).setOnComplete(()=> {
                    EventBus.TransitionEnded.Invoke();
                    Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " ends!");
                });
        }
    }

    private IEnumerator DuringTransition(float audioDuration)
    {
        yield return new WaitForSeconds(audioDuration);
        // Show prompt for User to walk out
        duringTransitionCoroutine = null;
    }

    private void HandleEnterStartTransitionTrigger(EnvironmentType toEnv)
    {
        if (!ChateauSceneManager.Instance.CalibrationIsFinished) return;
        startTransitionTrigger.gameObject.SetActive(false);
        endTransitionTrigger.gameObject.SetActive(true);
        EventBus.EnteredTranitionStartTrigger.Invoke(toEnv);
    }

    public void HandleEnterEndTransitionTrigger(EnvironmentType toEnv)
    {
        if (!ChateauSceneManager.Instance.CalibrationIsFinished) return;
        startTransitionTrigger.gameObject.SetActive(true);
        endTransitionTrigger.gameObject.SetActive(false);
        EventBus.EnteredTranitionEndTrigger.Invoke(toEnv);
    }

    public void UpdateForNextEnv(EnvironmentType nextEnv)
    {
        Debug.Log("update for next env: " + nextEnv.ToString());

        // update Transform
        transform.position = envToTransitionAnchor[nextEnv].transform.position;
        transform.rotation = envToTransitionAnchor[nextEnv].transform.rotation;

        // update labels of two Triggers
        startTransitionTrigger.UpdateToEnvType(nextEnv);
        endTransitionTrigger.UpdateToEnvType(nextEnv);
    }

    public void FadeInTransitionCube(EnvironmentType env)
    {
        FadeInTransitionCube(envToTransitionObject[env].interiorColor);
    }

    public LTDescr FadeInTransitionCube(Color newColor)
    {
        if (!transitionCube.activeSelf)
        {
            Color startColor = new Color(newColor.r, newColor.g, newColor.b, 0f);
            transitionCubeMaterial.color = startColor;
            transitionCube.SetActive(true);
        }
        return LeanTween.value(transitionCube, 0f, 1f, 1f)
            .setOnUpdate((float val) =>
            {
                transitionCubeMaterial.color = Color.Lerp(transitionCubeMaterial.color, newColor, val);
            });
    }

    public LTDescr FadeOutTransitionCube()
    {
        Color clearColor = transitionCubeMaterial.color;
        clearColor.a = 0f;
        return LeanTween.value(transitionCube, 0f, 1f, 1f)
            .setOnUpdate((float val) => {
                transitionCubeMaterial.color = Color.Lerp(transitionCubeMaterial.color, clearColor, val);
            });
    }

    public void Reset()
    {
        startTransitionTrigger.gameObject.SetActive(false);
        endTransitionTrigger.gameObject.SetActive(true);

        turnText.gameObject.SetActive(true);
        turnText.text = "Diatom lost.\nPlease turn around to restart the experience.";
    }
}
