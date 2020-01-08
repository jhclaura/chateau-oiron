using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerHelper : MonoBehaviour
{
    public GameObject m_modelOculusTouchQuestAndRiftSLeftController;
    public GameObject m_modelOculusTouchQuestAndRiftSRightController;
    public OVRInput.Controller m_controller;

    private bool m_prevControllerConnected = false;
    private bool m_prevControllerConnectedCached = false;

    void Start()
    {
        OVRPlugin.SystemHeadset headset = OVRPlugin.GetSystemHeadsetType();
        Debug.LogFormat("OVRControllerHelp: Active controller for product {0}", OVRPlugin.productName);
        if (m_controller == OVRInput.Controller.LTouch)
        {
            m_controller = OVRInput.Controller.LTrackedRemote;
        }
        else if (m_controller == OVRInput.Controller.RTouch)
        {
            m_controller = OVRInput.Controller.RTrackedRemote;
        }
    }
}
