using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "EnvSettings", menuName = "Environment/Settings")]
public class EnvironmentSettings : ScriptableObject
{
    [ReorderableList]
    public List<Environment> environmentOrder;
}
