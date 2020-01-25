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
    public Monologue monologue;
    public GameObject transitionCube;

    [Space(10)]
    public bool visibleTrigger;
    public TransitionTrigger startTransitionTrigger;
    public TransitionTrigger endTransitionTrigger;
    public GameObject introWall;
    public GameObject transitionWall;
    public GameObject turnGraphic;
    public TMPro.TextMeshPro turnText;
    public TMPro.TextMeshPro frontText;

    public Dictionary<EnvironmentType, TransitionScriptableObject> envToTransitionObject = new Dictionary<EnvironmentType, TransitionScriptableObject>();
    public Dictionary<EnvironmentType, TransitionAnchor> envToTransitionAnchor = new Dictionary<EnvironmentType, TransitionAnchor>();

    private EnvironmentType currentTransitionToEnv;
    private Material transitionCubeMaterial;
    private Material introWallMaterial;
    private Material transitionWallMaterial;
    private IEnumerator duringTransitionCoroutine;
    private AnimatedAudio animatedAudio;

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

        transitionWallMaterial = transitionWall.GetComponent<Renderer>().sharedMaterial;
        transitionWallMaterial.color = Color.black;
        transitionWall.SetActive(false);

        introWallMaterial = introWall.GetComponent<Renderer>().sharedMaterial;
        introWallMaterial.color = Color.black;
        introWall.SetActive(false);

        if (!visibleTrigger)
        {
            startTransitionTrigger.GetComponent<MeshRenderer>().enabled = false;
            endTransitionTrigger.GetComponent<MeshRenderer>().enabled = false;
        }

        animatedAudio = monologue.GetComponent<AnimatedAudio>();

        Reset();
    }

    // Called by ChateauSceneManager
    public void StartTransition(EnvironmentType env, bool skipFade = false, bool playAudio = true)
    {
        currentTransitionToEnv = env;

        if (playAudio)
        {
            animatedAudio.TargetAudio.clip = envToTransitionObject[env].audioClip;
            //animatedAudio.ToggleOn();
            animatedAudio.Play();
        }

        // Start waiting for transition to end, if not ended by User (by walking out of transition)
        duringTransitionCoroutine = DuringTransition(envToTransitionObject[env].audioClip.length);
        StartCoroutine(duringTransitionCoroutine);

        // Just show Transition Cube (case: Calibration Start)
        if (skipFade)
        {
            SetTransitionCubeColor(envToTransitionObject[env].interiorColor);
            EventBus.TransitionStarted.Invoke();
            //turnGraphic.SetActive(true);
            //turnText.gameObject.SetActive(true);
            Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " starts!");
        }
        else // Fade in Transition Cube
        {
            // Show transition wall abruptly
            ShowTransitionWall(env);

            LTDescr tween = FadeInTransitionCube(envToTransitionObject[env].interiorColor);
            tween.setOnComplete(() =>
            {
                EventBus.TransitionStarted.Invoke();
                if (env != EnvironmentType.Water)
                {
                    //turnGraphic.SetActive(true);
                    //turnText.gameObject.SetActive(true);
                }
                else
                {
                    frontText.gameObject.SetActive(true);
                }

                Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " starts!");
            });
        }

        if (currentTransitionToEnv == EnvironmentType.End)
        {
            frontText.text = "Closing starts. Proceed to end it.";
            frontText.color = Color.white;
            frontText.gameObject.SetActive(true);
            turnText.text = "";
        }
        else
        {
            if (env != EnvironmentType.Water)
                turnText.text = "Diatom lost.\nDiatom found.\nYou are now connected to the\nlabyrinth network.\nTurn around and step forward.";

            // diatome lost => instead, play animatedAudio from each TransitionObject
            //MonologueManager.Instance.Play(monologue);
        }
    }

    // Called by ChateauSceneManager
    public void EndTransition(bool doFadeOut = true, bool stopAudio = true)
    {
        if(stopAudio)
            animatedAudio.Stop(true, 0.5f);

        turnGraphic.SetActive(false);
        turnText.gameObject.SetActive(false);
        frontText.gameObject.SetActive(false);

        HideTransitionWall();

        if (currentTransitionToEnv == EnvironmentType.End)
        {
            frontText.text = "";
            frontText.gameObject.SetActive(false);
        }

        if (duringTransitionCoroutine != null)
        {
            StopCoroutine(duringTransitionCoroutine);
            duringTransitionCoroutine = null;
        }

        // Fade out Transition Cube
        if (doFadeOut)
        {
            LTDescr tween;
            Debug.Log(currentTransitionToEnv);
            if(currentTransitionToEnv==EnvironmentType.Water)
                tween = FadeOutTransitionCube(8f, 2.1f);
            else
                tween = FadeOutTransitionCube();

            tween.setOnComplete(() =>
            {
                transitionCube.SetActive(false);
                EventBus.TransitionEnded.Invoke();
                Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " ends!");
            });
        }
        else
        {
            // change Transition Cube to black
            LeanTween.value(transitionCube, 0f, 1f, 1f)
                .setOnUpdate((float val) =>
                {
                    transitionCubeMaterial.color = Color.Lerp(transitionCubeMaterial.color, Color.black, val);
                }).setOnComplete(() =>
                {
                    EventBus.TransitionEnded.Invoke();
                    Debug.Log("Transition to " + currentTransitionToEnv.ToString() + " ends!");
                });
        }
    }

    // Will be called by ChateauSceneManager, probably ONLY ChateauSceneManager
    public void PlayTransitionAudio(EnvironmentType env)
    {
        animatedAudio.TargetAudio.clip = envToTransitionObject[env].audioClip;
        animatedAudio.ToggleOn();
    }

    private IEnumerator DuringTransition(float audioDuration)
    {
        yield return new WaitForSeconds(audioDuration);
        // TODO: Show prompt for User to walk out
        duringTransitionCoroutine = null;
    }

    public void HandleEnterStartTransitionTrigger(EnvironmentType toEnv)
    {
        Debug.Log("Enter start transition trigger");
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

    public void UpdateAfterCalibrationEnd()
    {
        // fade out text?

        // update Transform
        transform.position = envToTransitionAnchor[currentTransitionToEnv].transform.position;
        transform.rotation = envToTransitionAnchor[currentTransitionToEnv].transform.rotation;

        // fade in text, show more stuff
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

    public LTDescr FadeOutTransitionCube(float speed = 1f, float delay = 0f)
    {
        Color clearColor = transitionCubeMaterial.color;
        clearColor.a = 0f;
        return LeanTween.value(transitionCube, 0f, 1f, speed)
            .setDelay(delay)
            .setOnUpdate((float val) =>
            {
                transitionCubeMaterial.color = Color.Lerp(transitionCubeMaterial.color, clearColor, val);
            });
    }

    private void SetTransitionCubeColor(Color newColor)
    {
        transitionCubeMaterial.color = newColor;
    }

    public void Reset()
    {
        startTransitionTrigger.gameObject.SetActive(false);
        endTransitionTrigger.gameObject.SetActive(false);   // first scene is triggered automatically by SceneManager

        //turnText.gameObject.SetActive(true);
        turnText.text = "Diatom lost.\nPlease turn around to restart the experience.";
    }

    public void ShowTransitionWall(EnvironmentType env)
    {
        // TODO: trigger different animations

        if (env==EnvironmentType.Water)
        {
            introWall.SetActive(true);
            LeanTween.value(introWall, 0f, 1f, 2f)
                    .setOnUpdate((float val) =>
                    {
                        introWallMaterial.color = Color.Lerp(introWallMaterial.color, Color.grey, val);
                    });
        }
        else
        {
            transitionWall.SetActive(true);
            transitionWallMaterial.color = Color.white;
        }
    }

    public void HideTransitionWall()
    {
        if (currentTransitionToEnv == EnvironmentType.Water)
        {
            LeanTween.value(introWall, 0f, 1f, 2f)
                    .setOnUpdate((float val) =>
                    {
                        introWallMaterial.color = Color.Lerp(introWallMaterial.color, Color.black, val);
                    })
                    .setOnComplete(()=>
                    {
                        introWall.SetActive(false);
                    });
        }
        else
        {
            transitionWall.SetActive(false);
        }
    }
}
