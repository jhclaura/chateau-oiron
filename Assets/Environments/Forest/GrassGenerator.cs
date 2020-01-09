using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    public GameObject grassPrefab;
    public int amount;

    void Start()
    {
        Vector3 randomPos;
        for (int i=0; i<amount; i++)
        {
            randomPos = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(0f, -21f));
            GameObject newGrass = Instantiate(grassPrefab, randomPos, Quaternion.AngleAxis(Random.Range(0f, 180f), Vector3.up));
            newGrass.transform.SetParent(transform, false);
        }
    }
}
