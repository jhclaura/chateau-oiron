using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VRHand
{
    Left,
    Right
}

public class VRPlatformManager : Manager<VRPlatformManager> {

    [Header("Oculus")]
    public OvrAvatar oculusAvatar;
    public GameObject oculusCameraRig;
    public GameObject oculusLeftController;
    public GameObject oculusLeftControllerModel;
    public GameObject oculusRightController;
    public GameObject oculusRightControllerModel;
    public GameObject oculusCenterCamera;

    private OVRScreenFade oVRScreenFade;
    private float lastTimescale = 1f;

    void Start()
    {
        InitializeOculus();
    }

    void InitializeOculus()
    {
        oculusCameraRig.SetActive(true);

        Oculus.Platform.Core.Initialize();
        Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(HandleEntitlementCheckComplete);

        OVRManager.display.RecenteredPose += HandleOculusRecenter;
        OVRManager.VrFocusLost += HandleOculusFocusLost;
        OVRManager.VrFocusAcquired += HandleOculusFocusAcquired;

        oVRScreenFade = oculusCenterCamera.GetComponent<OVRScreenFade>();
    }

    void HandleEntitlementCheckComplete(Oculus.Platform.Message msg)
    {
        if (msg.IsError)
        {
#if OCULUS_STORE
            Application.Quit();
#endif
        }
        else
        {
            oculusCameraRig.SetActive(true);
        }
    }

    private void HandleOculusRecenter()
    {
        OVRManager.display.RecenterPose();
    }

    private void HandleOculusFocusLost()
    {
        if (Time.timeScale != 0) lastTimescale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void HandleOculusFocusAcquired()
    {
        Time.timeScale = lastTimescale;
    }

    //public void ShowController(VRHand hand, bool show)
    //{
    //    if (hand == VRHand.Left)
    //    {
    //        oculusAvatar.ShowLeftController(show);
    //    } else
    //    {
    //        oculusAvatar.ShowRightController(show);
    //    }
    //}

    public void FadeOutCameraView()
    {
        oVRScreenFade.fadeTime = 1f;
        oVRScreenFade.FadeOut();
    }

    public void FadeInCameraView()
    {
        oVRScreenFade.fadeTime = 2f;
        StartCoroutine(oVRScreenFade.Fade(1, 0));
    }

    public Vector3 GetControllerPosition(VRHand hand, Vector3 offset = default(Vector3))
    {
        if (hand==VRHand.Left)
        {
            return oculusLeftController.transform.TransformPoint(offset);
        }
        else
        {
            return oculusRightController.transform.TransformPoint(offset);
        }
    }
}
