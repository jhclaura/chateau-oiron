using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GridCeiling : MonoBehaviour
{
    public bool devMode;
    [ShowIf("devMode")]
    public Transform dummyEnvTransform;
    [ShowIf("devMode")]
    public Transform playerCameraTransform;
    [Space(10)]
    public ChateauScene chateauScene;
    public Transform pivot;
    public float degreePerSecond = 1f;
    public bool doRotate;
    public AnimatedAudio mainEnvSound;

    private float envSoundLength;
    private Vector3 newPivotPosition;
    private Vector3 camOnFloor;
    private Vector3 pivotToFloorDirection;
    private Vector3 previousPlayerCameraPosition;
    private bool sceneStarted;

    private void Start()
    {
        // register events
        chateauScene.SceneStarted += OnSceneStart;

        envSoundLength = mainEnvSound.TargetAudio.clip.length;

        if (devMode)
        {
            // give random transform to the env
            chateauScene.UpdateTransformWithAnchor(dummyEnvTransform);
        }
        else
        {
            playerCameraTransform = VRPlatformManager.Instance.oculusCenterCamera.transform;
        }

        newPivotPosition = pivot.transform.position;            
        pivotToFloorDirection = new Vector3(0, 0-pivot.position.y, 0);
        camOnFloor = new Vector3(pivot.position.x, 0, playerCameraTransform.position.z);
        previousPlayerCameraPosition = playerCameraTransform.position;
    }

    void Update()
    {
        //if (!sceneStarted) return;

        Vector3 pivotOnFloor = new Vector3(pivot.transform.position.x, 0, pivot.transform.position.z);
        //Vector3 pivotOnFloorToCamDir = playerCameraTransform.position - pivotOnFloor;
        //pivotOnFloorToCamDir.y = 0;
        //Vector3 pivotForward = chateauScene.startPoint.forward;
        //Vector3 projection = Vector3.Project(pivotOnFloorToCamDir, pivotForward);
        //camOnFloor = pivotOnFloor + projection;

        //camOnFloor.z = playerCameraTransform.position.z;
        camOnFloor = ProjectPointOntoSelfForward(
            new Vector3(playerCameraTransform.position.x, 0, playerCameraTransform.position.z),
            pivotOnFloor,
            chateauScene.startPoint.forward);

        float angle = Vector3.Angle(camOnFloor - pivot.transform.position, pivotToFloorDirection);

        float autoAngle = degreePerSecond * Time.deltaTime;
        bool rotateForward = (playerCameraTransform.position.z - previousPlayerCameraPosition.z)<0 ? true : false;

        if (doRotate && angle < autoAngle)
        {
            pivot.RotateAround(pivot.transform.position, -pivot.transform.right, autoAngle);
        }
        else
        {
            if(rotateForward)
                pivot.RotateAround(pivot.transform.position, -pivot.transform.right, angle);
            else
                pivot.RotateAround(pivot.transform.position, pivot.transform.right, angle);
        }

        // Follow player camera movement
        //newPivotPosition.z = playerCameraTransform.position.z;
        newPivotPosition.x = camOnFloor.x;
        newPivotPosition.z = camOnFloor.z;
        pivot.transform.position = newPivotPosition;
        previousPlayerCameraPosition = playerCameraTransform.position;
    }

    private Vector3 ProjectPointOntoSelfForward(Vector3 target, Vector3 selfPosition, Vector3 selfForward)
    {
        Vector3 v1 = target - selfPosition;
        Vector3 v2 = Vector3.Project(v1, selfForward);
        return selfPosition + v2;
    }

    public void OnSceneStart()
    {
        camOnFloor = new Vector3(pivot.position.x, 0, playerCameraTransform.position.z);
        previousPlayerCameraPosition = playerCameraTransform.position;

        //mainEnvSound.ToggleOn();
        doRotate = true;
        sceneStarted = true;
    }

    public void TriggerEnding()
    {
        doRotate = false;
        ChateauSceneManager.Instance.TheTransitionGroup.HandleEnterStartTransitionTrigger(EnvironmentType.End);
    }
}
