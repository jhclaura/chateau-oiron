using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcVisualizerQuest : MonoBehaviour
{
    [Tooltip("Raycaster to visualize")]
    public ArcRaycasterQuest arcRaycaster;
    [Tooltip("Line renderer used for visualization")]
    public LineRenderer arcRenderer;
    [Tooltip("Game object indicating when the raycaster hit something")]
    public Transform contactIndicator;
    [Tooltip("Game object indicating direction of raycast")]
    public Transform directionIndicator;

    [Tooltip("How many segments to use for curve, must be at least 3. More segments = better quality")]
    public int segments = 20;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
