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
    }

    void Update()
    {
        newPosition = cameraTransform.position;
        headCollider.position = newPosition;

        newPosition.y = transform.position.y;
        bodyCollider.position = newPosition;
    }
}
