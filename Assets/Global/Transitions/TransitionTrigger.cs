using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    public System.Action<EnvironmentType> OnEnterTrigger;
    private EnvironmentType toEnv;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            if (OnEnterTrigger != null) OnEnterTrigger(toEnv);
        }
    }

    public void UpdateToEnvType(EnvironmentType newToEnv)
    {
        toEnv = newToEnv;
    }
}
