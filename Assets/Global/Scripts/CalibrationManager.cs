using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationManager : Manager<CalibrationManager>
{
    public Transform startAnchor;
    public Transform environmentHolder;
    public GameObject forwardPrefab;
    public Vector3 leftControllerOffset;
    public Vector3 rightControllerOffset;

    [Header("Dev")]
    public bool devMode;
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

    void Start()
    {
        StartCoroutine(Calibrate());
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
        if (!leftControllerIsPressed)
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

            //if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            //{
            //    leftControllerPressedTime += Time.deltaTime;
            //    if (leftControllerPressedTime > 1f)
            //    {
            //        leftControllerIsPressed = true;
            //        VibrateController(OVRInput.Controller.LTouch);
            //    }
            //}
            //else if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) && !OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            //{
            //    leftControllerPressedTime = 0;
            //}

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

            //if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            //{
            //    rightControllerPressedTime += Time.deltaTime;
            //    if (leftControllerPressedTime > 1f)
            //    {
            //        rightControllerIsPressed = true;
            //        VibrateController(OVRInput.Controller.RTouch);
            //    }
            //}
            //else if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) && !OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            //{
            //    rightControllerPressedTime = 0;
            //}
        }
    }

    IEnumerator Calibrate()
    {
        // waiting for two controller both be PRESSED
        while (!leftControllerIsPressed || !rightControllerIsPressed)
        {
            yield return null;
        }
        DisplayInfoText("Start calibration!", 2f);
        EventBus.CalibrationStarted.Invoke();

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

        while (passedTime < 5f)
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

        rightControllerPosition.y = leftControllerPosition.y;

        Vector3 centerPoint = (leftControllerPosition + rightControllerPosition) / 2f;
        Vector3 side1 = leftControllerPosition - centerPoint;
        //Vector3 side2 = rightControllerPosition - centerPoint;
        Vector3 direction = Quaternion.Euler(0, 90, 0) * side1;
        direction.Normalize();

        startAnchor.position = centerPoint;
        startAnchor.rotation = Quaternion.LookRotation(direction);

        GameObject newForward = Instantiate(forwardPrefab, centerPoint, Quaternion.LookRotation(direction), environmentHolder);

        Debug.DrawRay(centerPoint, direction, Color.green, 5f);

        DisplayInfoText("Finish calibration!", 2f);
        EventBus.CalibrationEnded.Invoke();
    }

    public void VibrateController(OVRInput.Controller controller)
    {
        StartCoroutine(DoControllerVibration(controller));
    }

    private IEnumerator DoControllerVibration(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, 0.5f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.0f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.5f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.0f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.5f, controller);
        yield return controllerVibrationWait;
        OVRInput.SetControllerVibration(1f, 0.0f, controller);
        yield return controllerVibrationWait;
    }

    public void DisplayInfoText(string info, float duration=2f)
    {
        if (infoText.text != "")
        {
            infoText.text += ("\n" + info);
        }
        else
        {
            infoText.text = info;
        }
        Debug.Log(info);

        CancelInvoke("EmptyInfoText");
        Invoke("EmptyInfoText", duration);
    }

    private void EmptyInfoText()
    {
        infoText.text = "";
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

        StartCoroutine(Calibrate());
    }
}
