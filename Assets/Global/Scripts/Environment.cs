using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Env", menuName = "Environment/Env", order = 1)]
public class Environment : ScriptableObject
{
    public EnvironmentType environmentType;
    public string sceneName;
    [Space(10)]
    public int fogStart;
    public int fogEnd;
    public Color fogColor;
    public Color[] skyColors;
    public Color[] ambientColors;
    public Color cameraFadeOutColor;
}
