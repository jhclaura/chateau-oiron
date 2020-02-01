using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScene : MonoBehaviour
{
    public GameObject frontWall;

    private void Awake()
    {
        frontWall.SetActive(false);
    }
}
