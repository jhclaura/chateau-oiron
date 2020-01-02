using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ChateauSceneManager : Manager<ChateauSceneManager>
{
    public Transform anchor;
    public Transform transitionGroupObject;
    public EnvironmentSettings environmentSettings;
    [ReadOnly]
    public EnvironmentType currentEnvironment;

    [Space(10)]
    public bool restartAfterEnd;

    private int nextEnvIndex = 0;
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
        //environmentOrderDict = new Dictionary<int, EnvironmentType>()
        //{
        //    { 1, EnvironmentType.Fire },
        //    { 2, EnvironmentType.Water },
        //    { 3, EnvironmentType.Forest },
        //    { 4, EnvironmentType.Beetle },
        //    { 5, EnvironmentType.End }
        //};
        transitionGroup = transitionGroupObject.GetComponent<TransitionGroup>();

        // Enable Opening transition cube ONLY, don't start it yet
        transitionGroup.FadeInTransitionCube(currentEnvironment);
        EnvironmentManager.Instance.LoadScene(currentEnvironment);

        CalibrationManager.Instance.StartCalibration();
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

        // update Transition Group transform
        transitionGroup.UpdateAfterCalibrationEnd();
        // update env transformation
        currentChateauScene.UpdateTransformWithAnchor(transitionGroup.envToTransitionAnchor[EnvironmentManager.Instance.currentEnvironment].transform);
    }

    private void HandleNewSceneLoaded(string newEnv)
    {
        currentChateauScene = FindObjectOfType<ChateauScene>();
        Debug.Log("Found currentChateauScene of " + currentChateauScene.gameObject.name);

        //switch (newEnv)
        //{
        //    case "fire":

        //        break;

        //    case "water":
        //    case "forest":
        //    case "beetle":

        //        break;
        //}

        // update env transformation
        currentChateauScene.UpdateTransformWithAnchor(transitionGroup.envToTransitionAnchor[EnvironmentManager.Instance.currentEnvironment].transform);
    }

    private void HandleEnterStartTransitionTrigger(EnvironmentType toEnv)
    {
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
        if (toEnv != EnvironmentType.End)
        {
            // turn off transition
            transitionGroup.EndTransition();

            // update env transformation
            //currentChateauScene.UpdateTransformWithAnchor(transitionGroup.envToTransitionAnchor[toEnv].transform);

            // turn on env
            currentChateauScene.ActivateScene();
        }
        else
        {
            // turn off transition + fade to black 
            transitionGroup.EndTransition(false);
        }
    }

    private void HandleTransitionEnded()
    {
        // update TransitionGroup for next env
        nextEnvIndex++;

        if (nextEnvIndex < environmentSettings.environmentOrder.Count)
        {
            currentEnvironment = environmentSettings.environmentOrder[nextEnvIndex].environmentType;
            transitionGroup.UpdateForNextEnv(currentEnvironment);
        }
        else
        {
            // End ends!!!
            if(restartAfterEnd)
            {
                Reset();
                Restart();
            }
        }
    }

    private void Reset()
    {
        m_calibrationIsFinished = false;
        nextEnvIndex = 0;
        currentEnvironment = environmentSettings.environmentOrder[nextEnvIndex].environmentType;

        CalibrationManager.Instance.Reset();
        CalibrationManager.Instance.StartCalibration();
    }

    private void Restart()
    {
        transitionGroup.FadeInTransitionCube(currentEnvironment);
        EnvironmentManager.Instance.HandleEnvironmentChange(currentEnvironment);
        transitionGroup.UpdateForNextEnv(currentEnvironment);
        transitionGroup.Reset();
    }
}
