using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeStickyFollow : MonoBehaviour
{
    public bool autoFollow;
    public bool followRotation = true;
    private Quaternion newRot;

    void Update()
    {
        if (autoFollow)
        {
            transform.position = Camera.main.transform.position;
            if (followRotation)
            {
                newRot = Camera.main.transform.rotation;
                newRot.x = newRot.z = 0;
                transform.rotation = newRot;
            }
        }
    }
}
