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
        Vector3 childOffset = referenceStartAnchor.position - startPoint.position;

        transform.rotation = referenceStartAnchor.rotation;
        transform.position += childOffset;
    }

    public void ActivateScene()
    {

    }

    public void DeactivateScene()
    {

    }
}
