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
    private Vector3 newPivotPosition;
    //[HideInInspector]
    public Transform playerCameraTransform;
    private Vector3 camOnFloor;
    private Vector3 floorToPivot;
    private Vector3 previousPlayerCameraPosition;
    private bool sceneStarted;

    private void Start()
    {
        envSoundLength = mainEnvSound.TargetAudio.clip.length;
        chateauScene.SceneStarted += OnSceneStart;
        newPivotPosition = pivot.transform.position;
        if (playerCameraTransform==null)
            playerCameraTransform = VRPlatformManager.Instance.oculusCenterCamera.transform;
        floorToPivot = new Vector3(0,-pivot.position.y,0);
    }

    void Update()
    {
        if (!sceneStarted) return;

        camOnFloor.z = playerCameraTransform.position.z;
        //Vector3 camToPivot = (camOnFloor - pivot.transform.position);
        float angle = Vector3.Angle(camOnFloor - pivot.transform.position, floorToPivot);
        //Debug.Log(angle);

        float autoAngle = degreePerSecond * Time.deltaTime;
        bool rotateForward = (playerCameraTransform.position.z - previousPlayerCameraPosition.z)<0 ? true : false;

        if (doRotate && angle < autoAngle)
        {
            pivot.RotateAround(pivot.transform.position, -Vector3.right, autoAngle);
        }
        else
        {
            if(rotateForward)
                pivot.RotateAround(pivot.transform.position, -Vector3.right, angle);
            else
                pivot.RotateAround(pivot.transform.position, Vector3.right, angle);
        }

        // Follow player camera movement
        newPivotPosition.z = playerCameraTransform.position.z;
        pivot.transform.position = newPivotPosition;
        previousPlayerCameraPosition = playerCameraTransform.position;
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
