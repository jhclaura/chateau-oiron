using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransitionOf", menuName = "ScriptableObjects/TransitionScriptableObject", order = 1)]
public class TransitionScriptableObject : ScriptableObject
{
    public EnvironmentType toEnvironment;
    public AudioClip audioClip;
    public Color interiorColor = Color.black;
}
