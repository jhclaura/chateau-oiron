using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeStickyFollow : MonoBehaviour
{
    public bool autoFollow;
    private Quaternion newRot;

    void Update()
    {
        if (autoFollow)
        {
            transform.position = Camera.main.transform.position;
            newRot = Camera.main.transform.rotation;
            newRot.x = newRot.z = 0;
            transform.rotation = newRot;
        }
    }
}
