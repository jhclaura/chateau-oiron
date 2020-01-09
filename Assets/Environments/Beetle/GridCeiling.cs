using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCeiling : MonoBehaviour
{
    public ChateauScene chateauScene;
    public Transform pivot;
    public float degreePerSecond = 1f;
    public bool doRotate;
    public AnimatedAudio mainEnvSound;

    private float envSoundLength;

    private void Start()
    {
        envSoundLength = mainEnvSound.TargetAudio.clip.length;
        chateauScene.SceneStarted += OnSceneStart;
    }

    void Update()
    {
        if (doRotate)
        {
            pivot.RotateAround(pivot.transform.position, -Vector3.right, degreePerSecond * Time.deltaTime);
        }
    }

    public void OnSceneStart()
    {
        mainEnvSound.ToggleOn();
        doRotate = true;
    }

    public void TriggerEnding()
    {
        doRotate = false;
        ChateauSceneManager.Instance.TheTransitionGroup.HandleEnterStartTransitionTrigger(EnvironmentType.End);
    }
}
