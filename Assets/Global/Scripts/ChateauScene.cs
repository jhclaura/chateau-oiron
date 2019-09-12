using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChateauScene : MonoBehaviour
{
    public Transform startPoint;

    private void Awake()
    {
        DeactivateScene();
    }

    public void UpdateTransformWithAnchor(Transform referenceStartAnchor)
    {
        transform.rotation = referenceStartAnchor.rotation * Quaternion.Inverse(startPoint.rotation);
        Vector3 childOffset = referenceStartAnchor.position - startPoint.position;
        transform.position += childOffset;
    }

    public void ActivateScene()
    {

    }

    public void DeactivateScene()
    {

    }
}
