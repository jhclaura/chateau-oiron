﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationManager : Manager<CalibrationManager>
{
    public Transform startAnchor;
    public Transform environmentHolder;
    public GameObject centralLine;
    public Vector3 leftControllerOffset;
    public Vector3 rightControllerOffset;
    public float waitTimeAfterCalibrationEnds = 30f;

    //[Header("Sounds")]
    //public Monologue waitMonologue;
    //public Monologue proceedMonologue;

    [Header("Dev")]
    public bool devMode;
    private bool rightControllerOnly = true;
    private bool lookStraightMode;
    public Transform dummyControllerLeft;
    public Transform dummyControllerRight;
    public GameObject smallBallPrefab;
    public Material redMaterial;
    public Material greenMaterial;
    public TMPro.TextMeshPro infoText;

    private Vector3 leftControllerPosition;
    private Vector3 rightControllerPosition;
    private GameObject leftSmallBall;
    private GameObject rightSmallBall;

    private bool leftControllerIsPressed;
    private bool rightControllerIsPressed;
    private float leftControllerPressedTime;
    private float rightControllerPressedTime;
    private WaitForSeconds controllerVibrationWait;
    private Color infoTextColor;
    private int emptyInfoTweenId;

    void Start()
    {
        infoTextColor = infoText.color;
        leftSmallBall = Instantiate(smallBallPrefab, transform);
        rightSmallBall = Instantiate(smallBallPrefab, transform);
        controllerVibrationWait = new WaitForSeconds(0.2f);
    }

    void Update()
    {
        if (leftSmallBall.activeSelf && rightSmallBall.activeSelf)
        {
            if (devMode)
            {
                leftSmallBall.transform.position = dummyControllerLeft.position;
                rightSmallBall.transform.position = dummyControllerRight.position;
            }
            else
            {
                leftSmallBall.transform.position = VRPlatformManager.Instance.GetControllerPosition(VRHand.Left, leftControllerOffset);
                rightSmallBall.transform.position = VRPlatformManager.Instance.GetControllerPosition(VRHand.Right, rightControllerOffset);
            }
        }

        // Listen for controllers
        if (!rightControllerOnly && !leftControllerIsPressed)
        {
            if (CheckIfPressingController(VRHand.Left))
            {
                leftControllerPressedTime += Time.deltaTime;
                if (leftControllerPressedTime > 1f)
                {
                    leftControllerIsPressed = true;
                    VibrateController(OVRInput.Controller.LTouch);
                    DisplayInfoText("Left Controller is pressed.");
                }
            }
            else
            {
                leftControllerPressedTime = 0;
            }
        }

        if (!rightControllerIsPressed)
        {
            if (CheckIfPressingController(VRHand.Right))
            {
                rightControllerPressedTime += Time.deltaTime;
                if (rightControllerPressedTime > 1f)
                {
                    rightControllerIsPressed = true;
                    VibrateController(OVRInput.Controller.RTouch);
                    DisplayInfoText("Right Controller is pressed.");
                }
            }
            else
            {
                rightControllerPressedTime = 0;
            }
        }
    }

    public void StartCalibration()
    {
        StartCoroutine(Calibrate());
    }

    IEnumerator Calibrate()
    {
        // waiting for two controller both be PRESSED
        float timePassed = 0;
        bool displayInfoText = false;
        while ((!rightControllerOnly && !leftControllerIsPressed) || !rightControllerIsPressed)
        {
            timePassed += Time.deltaTime;
            if (timePassed > 5f && !displayInfoText)
            {
                displayInfoText = true;
                if(rightControllerOnly)
                    DisplayInfoText("Now, please press right controller to start calibration.", 30f);
                else
                    DisplayInfoText("Now, please press both controllers to start calibration.", 30f);
            }
            yield return null;
        }
        DisplayInfoText("Calibration started.");
        EventBus.CalibrationStarted.Invoke();
        centralLine.SetActive(true);

        // after both are pressed, wait for 5 seconds
        float passedTime = 0;
        if (devMode)
        {
            leftControllerPosition = dummyControllerLeft.position;
            rightControllerPosition = dummyControllerRight.position;
        }
        else
        {
            leftControllerPosition = VRPlatformManager.Instance.GetControllerPosition(VRHand.Left, leftControllerOffset);
            rightControllerPosition = VRPlatformManager.Instance.GetControllerPosition(VRHand.Right, rightControllerOffset);
        }

        while (passedTime < 6f)
        {
            if (devMode)
            {
                leftControllerPosition = (leftControllerPosition + dummyControllerLeft.position) / 2f;
                rightControllerPosition = (rightControllerPosition + dummyControllerRight.position) / 2f;
            }
            else
            {
                leftControllerPosition = (leftControllerPosition + VRPlatformManager.Instance.GetControllerPosition(VRHand.Left, leftControllerOffset)) / 2f;
                rightControllerPosition = (rightControllerPosition + VRPlatformManager.Instance.GetControllerPosition(VRHand.Right, rightControllerOffset)) / 2f;
            }

            passedTime += Time.deltaTime;
            yield return null;
        }

        leftSmallBall.SetActive(false);
        rightSmallBall.SetActive(false);
        VRPlatformManager.Instance.oculusLeftControllerModel.SetActive(false);
        VRPlatformManager.Instance.oculusRightControllerModel.SetActive(false);

        rightControllerPosition.y = leftControllerPosition.y = 0;

        Vector3 centerPoint = (leftControllerPosition + rightControllerPosition) / 2f;
        Vector3 side1 = leftControllerPosition - centerPoint;
        //Vector3 side2 = rightControllerPosition - centerPoint;
        Vector3 direction = Quaternion.Euler(0, 90, 0) * side1;
        direction.Normalize();

        if (!lookStraightMode)
        {
            Debug.Log("startAnchor update!");
            startAnchor.position = centerPoint;
            startAnchor.rotation = Quaternion.LookRotation(direction);
        }

        DisplayInfoText("Calibration finished.");
        EventBus.CalibrationEnded.Invoke();

        yield return new WaitForSeconds(5f);
        //DisplayInfoText("Diatom lost.\nDiatom found.\nYou are now connected to the\nlabyrinth network. Proceed with caution.", 5f);
        //MonologueManager.Instance.Play(proceedMonologue);
        centralLine.SetActive(false);
        infoText.gameObject.SetActive(false);
        infoText.color = infoTextColor;

        // TODO: show intro visuals

        if(devMode)
            yield return new WaitForSeconds(1f);
        else
            yield return new WaitForSeconds(waitTimeAfterCalibrationEnds);

        // Start Intro! TODO: wait for a trigger press?
        Debug.Log("Start Experience Triggered, 30 sec after calibration");
        EventBus.StartExperienceTriggered.Invoke();
    }

    public void VibrateController(OVRInput.Controller controller)
    {
        StartCoroutine(DoControllerVibration(controller));
    }

    private IEnumerator DoControllerVibration(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, 0.8f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.0f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.8f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.0f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.8f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.0f, controller);
        yield return controllerVibrationWait;
    }

    public void DisplayInfoText(string info, float duration=3f)
    {
        Debug.Log(info);

        // if it's not dev mode, show no text
        //if (!devMode) return;

        if (emptyInfoTweenId != 0 && LeanTween.isTweening(emptyInfoTweenId))
        {
            LeanTween.cancel(emptyInfoTweenId);
        }

        if (infoText.text != "")
        {
            LeanTween.value(infoText.gameObject, infoText.color, Color.clear, .5f)
                .setOnUpdate(UpdateTextColor)
                .setOnComplete(()=> {
                    infoText.text = info;
                    LeanTween.value(infoText.gameObject, infoText.color, infoTextColor, 1f).setOnUpdate(UpdateTextColor);
                });
        }
        else
        {
            infoText.text = info;
            if (infoText.color != infoTextColor)
            {
                LeanTween.value(infoText.gameObject, infoText.color, infoTextColor, 1f).setOnUpdate(UpdateTextColor);
            }
        }

        EmptyInfoText(duration);
    }

    private void EmptyInfoText(float duration)
    {
        emptyInfoTweenId = LeanTween.value(infoText.gameObject, infoText.color, Color.clear, .5f)
            .setDelay(duration)
            .setOnUpdate(UpdateTextColor)
            .setOnComplete(()=> {
                infoText.text = "";
            }).id;
    }

    private void UpdateTextColor(Color col)
    {
        infoText.color = col;
    }

    private bool CheckIfPressingController(VRHand hand)
    {
        if (hand==VRHand.Left)
        {
            if (devMode)
            {
                return Input.GetKey("l");
            }
            else
            {
                return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
            }
        }
        else
        {
            if (devMode)
            {
                return Input.GetKey("r");
            }
            else
            {
                return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
            }
        }
    }

    public void Reset()
    {
        leftControllerPressedTime = 0;
        rightControllerPressedTime = 0;
        leftControllerIsPressed = false;
        rightControllerIsPressed = false;
        leftSmallBall.SetActive(true);
        rightSmallBall.SetActive(true);
        infoText.gameObject.SetActive(true);
        DisplayInfoText("Prepare for calibration.", 10f);
    }
}
