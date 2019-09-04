using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChateauSceneManager : Manager<ChateauSceneManager>
{
    public Transform anchor;
    public Transform transitionGroupObject;
    public EnvironmentType currentEnvironment = EnvironmentType.Fire;

    private Dictionary<int, EnvironmentType> environmentOrderDict;
    private int nextEnvIndex = 1;
    private bool m_calibrationIsFinished;
    public bool CalibrationIsFinished { get { return m_calibrationIsFinished; } }
    private ChateauScene currentChateauScene;
    private TransitionGroup transitionGroup;

    private void OnEnable()
    {
        EventBus.NewSceneLoaded.AddListener(HandleNewSceneLoaded);
        EventBus.CalibrationStarted.AddListener(HandleCalibrationStart);
        EventBus.CalibrationEnded.AddListener(HandleCalibrationEnd);
        EventBus.EnteredTranitionStartTrigger.AddListener(HandleEnterStartTransitionTrigger);
        EventBus.EnteredTranitionEndTrigger.AddListener(HandleEnterEndTransitionTrigger);
        EventBus.TransitionEnded.AddListener(HandleTransitionEnded);
    }

    private void OnDisable()
    {
        EventBus.NewSceneLoaded.RemoveListener(HandleNewSceneLoaded);
        EventBus.CalibrationStarted.RemoveListener(HandleCalibrationStart);
        EventBus.CalibrationEnded.RemoveListener(HandleCalibrationEnd);
        EventBus.EnteredTranitionStartTrigger.RemoveListener(HandleEnterStartTransitionTrigger);
        EventBus.EnteredTranitionEndTrigger.RemoveListener(HandleEnterEndTransitionTrigger);
        EventBus.TransitionEnded.RemoveListener(HandleTransitionEnded);
    }

    void Start()
    {
        environmentOrderDict = new Dictionary<int, EnvironmentType>()
        {
            { 1, EnvironmentType.Fire },
            { 2, EnvironmentType.Water },
            { 3, EnvironmentType.Forest },
            { 4, EnvironmentType.Beetle },
            { 5, EnvironmentType.End }
        };
        transitionGroup = transitionGroupObject.GetComponent<TransitionGroup>();

        // Enable Opening transition cube ONLY, don't start it yet
        transitionGroup.FadeInTransitionCube(currentEnvironment);
        EnvironmentManager.Instance.LoadScene(currentEnvironment);
    }

    private void HandleCalibrationStart()
    {
        m_calibrationIsFinished = false;
        transitionGroup.StartTransition(currentEnvironment, true);
    }

    private void HandleCalibrationEnd()
    {
        Debug.Log("Handle Calibration End!");
        m_calibrationIsFinished = true;
    }

    private void HandleNewSceneLoaded(string newEnv)
    {
        currentChateauScene = FindObjectOfType<ChateauScene>();
        CalibrationManager.Instance.DisplayInfoText("Found currentChateauScene of " + currentChateauScene.gameObject.name);

        //switch (newEnv)
        //{
        //    case "fire":

        //        break;

        //    case "water":
        //    case "forest":
        //    case "beetle":

        //        break;
        //}
    }

    private void HandleEnterStartTransitionTrigger(EnvironmentType toEnv)
    {
        //if (toEnv==EnvironmentType.Fire)
        //{
        //    // special case for Opening => start Fire scene by manually call EnterEndTransitionTrigger
        //    transitionGroup.HandleEnterEndTransitionTrigger(toEnv);
        //    return;
        //}

        // turn off currentEnv
        currentChateauScene.DeactivateScene();

        // transition starts (TODO: might need to wait for scene loaded due to performance)
        transitionGroup.StartTransition(toEnv);

        if (toEnv != EnvironmentType.End)
        {
            // It will 1) fade out to black; 2)unload currentEnv; 3)load nextEnv
            EnvironmentManager.Instance.HandleEnvironmentChange(toEnv);
        }
    }

    private void HandleEnterEndTransitionTrigger(EnvironmentType toEnv)
    {
        // turn off transition
        transitionGroup.EndTransition();

        if (toEnv != EnvironmentType.End)
        {
            // update env transformation
            currentChateauScene.UpdateTransformWithAnchor(transitionGroup.envToTransitionAnchor[toEnv].transform);

            // turn on env
            currentChateauScene.ActivateScene();
        }
        else
        {
            //
        }
    }

    private void HandleTransitionEnded()
    {
        // update TransitionGroup for next env
        nextEnvIndex++;

        if (environmentOrderDict.TryGetValue(nextEnvIndex, out EnvironmentType envType))
        {
            transitionGroup.UpdateForNextEnv(envType);
        }
        else
        {
            // End ends!!!
            // TEMP: restart
            Reset();

            // Enable Opening transition cube ONLY, don't start it yet
            transitionGroup.FadeInTransitionCube(currentEnvironment);
            EnvironmentManager.Instance.HandleEnvironmentChange(currentEnvironment);
            transitionGroup.UpdateForNextEnv(environmentOrderDict[nextEnvIndex]);
            transitionGroup.Reset();
        }
    }

    private void Reset()
    {
        m_calibrationIsFinished = false;
        CalibrationManager.Instance.Reset();
        nextEnvIndex = 1;
        currentEnvironment = EnvironmentType.Fire;
    }
}
