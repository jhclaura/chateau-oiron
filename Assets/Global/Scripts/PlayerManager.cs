using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Transform headCollider;
    public Transform bodyCollider;

    private Transform cameraTransform;
    private Vector3 newPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;

#if UNITY_EDITOR
        VRPlatformManager.Instance.oculusCenterCamera.transform.localPosition = Vector3.up * 1.8f;
#endif
    }

    void Update()
    {
        newPosition = cameraTransform.position;
        headCollider.position = newPosition;

        newPosition.y = transform.position.y;
        bodyCollider.position = newPosition;
    }
}
