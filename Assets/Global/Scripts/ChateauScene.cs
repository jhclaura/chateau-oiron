using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChateauScene : MonoBehaviour
{
    public System.Action SceneStarted;

    public Transform startPoint;
    [Space(10)]
    public AnimatedAudio[] aniAudios;
    public AnimatedLight[] aniLights;
    public GameObject[] sceneObjects;

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
        Debug.Log("Activate Scene");
        foreach (GameObject s_object in sceneObjects)
        {
            s_object.SetActive(true);
        }

        foreach (AnimatedAudio _audios in aniAudios)
        {
            _audios.ToggleOn();
        }

        foreach (AnimatedLight _light in aniLights)
        {
            _light.TurnOn(1f);
        }

        SceneStarted?.Invoke();
    }

    public void DeactivateScene()
    {
        Debug.Log("Deactivate Scene");

        foreach (GameObject s_object in sceneObjects)
        {
            s_object.SetActive(false);
        }

        foreach (AnimatedAudio _audios in aniAudios)
        {
            _audios.Stop(true);
        }

        foreach (AnimatedLight _light in aniLights)
        {
            _light.TurnOff();
        }
    }
}
