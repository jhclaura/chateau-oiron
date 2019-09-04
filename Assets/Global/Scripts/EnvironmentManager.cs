using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EnvironmentType
{
    Fire,
    Water,
    Forest,
    Beetle,
    End
}

[System.Serializable]
public struct EnvironmentColor
{
    public string name;
    public int fogStart;
    public int fogEnd;
    public Color fogColor;
    public Color[] skyColors;
    public Color[] ambientColors;
    public Color cameraFadeOutColor;
}

public class EnvironmentManager : Manager<EnvironmentManager>
{
    private EnvironmentType currentEnvironment;// = EnvironmentType.Fire;
    public Material skyboxMaterial;
    public EnvironmentColor[] environmentColors;

    private bool isLoading;
    private OVRScreenFade oVRScreenFade;
    private Dictionary<EnvironmentType, string> environmentSceneNameDictionary;
    private Dictionary<string, EnvironmentType> environmentTypeDictionary;
    private Dictionary<EnvironmentType, EnvironmentColor> environmentColorDictionary;

    void Awake()
    {
        environmentSceneNameDictionary = new Dictionary<EnvironmentType, string>()
        {
            { EnvironmentType.Fire, "Scene_fire" },
            { EnvironmentType.Water, "Scene_water" },
            { EnvironmentType.Forest, "Scene_forest" },
            { EnvironmentType.Beetle, "Scene_beetle" }
        };

        environmentTypeDictionary = new Dictionary<string, EnvironmentType>()
        {
            { "fire", EnvironmentType.Fire },
            { "water", EnvironmentType.Water },
            { "forest", EnvironmentType.Forest },
            { "beetle", EnvironmentType.Beetle }
        };

        environmentColorDictionary = new Dictionary<EnvironmentType, EnvironmentColor>()
        {
            { EnvironmentType.Fire, environmentColors[0] },
            { EnvironmentType.Water, environmentColors[1] },
            { EnvironmentType.Forest, environmentColors[2] },
            { EnvironmentType.Beetle, environmentColors[3] }
        };

        oVRScreenFade = VRPlatformManager.Instance.oculusCenterCamera.GetComponent<OVRScreenFade>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(EnvironmentType newEnv)
    {
        currentEnvironment = newEnv;
        SceneManager.LoadSceneAsync(environmentSceneNameDictionary[currentEnvironment], LoadSceneMode.Additive);
        UpdateEnvironmentColors();
    }

    public void HandleEnvironmentChange(EnvironmentType newEnvType)
    {
        Debug.Log("Handle Environment Change: " + newEnvType.ToString());
        if (newEnvType == currentEnvironment) return;
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(LoadEnvironmentSceneAsync(newEnvType));
    }

    public void HandleEnvironmentChange(string newEnv)
    {
        HandleEnvironmentChange(environmentTypeDictionary[newEnv]);
    }

    IEnumerator LoadEnvironmentSceneAsync(EnvironmentType newEnv)
    {
        FadeOut(environmentColorDictionary[currentEnvironment].cameraFadeOutColor);
        yield return new WaitForSeconds(1f);

        // Unload
        if (environmentSceneNameDictionary.ContainsKey(currentEnvironment))
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(environmentSceneNameDictionary[currentEnvironment]);
            // wait until the async scene fully unloads
            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        // Load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(environmentSceneNameDictionary[newEnv], LoadSceneMode.Additive);
        // wait until the async scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Call event
        currentEnvironment = newEnv;
        

        // change env colors
        UpdateEnvironmentColors();

        // Scene load! fade in camera view
        FadeIn();

        isLoading = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "_BaseScene") return;
        string envName = GetEnvrionmentName(currentEnvironment);       
        EventBus.NewSceneLoaded.Invoke(envName);
    }

    void UpdateEnvironmentColors()
    {
        //if (currentEnvironment == EnvironmentType.Bathhouse || currentEnvironment == EnvironmentType.EggBarn)
        //{
        //    RenderSettings.fog = true;
        //    RenderSettings.fogColor = environmentColorDictionary[currentEnvironment].fogColor;
        //    RenderSettings.fogStartDistance = environmentColorDictionary[currentEnvironment].fogStart;
        //    RenderSettings.fogEndDistance = environmentColorDictionary[currentEnvironment].fogEnd;
        //}
        //else
        //{
        //    RenderSettings.fog = false;
        //}

        EnvironmentColor colors = environmentColorDictionary[currentEnvironment];

        skyboxMaterial.SetColor("_Color2", colors.skyColors[0]);
        skyboxMaterial.SetColor("_Color1", colors.skyColors[1]);
        RenderSettings.ambientSkyColor = colors.ambientColors[0];
        RenderSettings.ambientEquatorColor = colors.ambientColors[0];
        RenderSettings.ambientGroundColor = colors.ambientColors[0];

    }

    public string GetEnvrionmentName(EnvironmentType env)
    {
        if (environmentSceneNameDictionary.TryGetValue(env, out string envName))
        {
            return envName;
        }
        else
        {
            return "";
        }
    }

    public void FadeOut(Color fadeColor, float fadeTime = 1f)
    {
        if (oVRScreenFade)
        {
            oVRScreenFade.fadeColor = fadeColor;
            oVRScreenFade.fadeTime = fadeTime;
            oVRScreenFade.FadeOut();
        }
    }

    public void FadeIn(float fadeTime = 2f)
    {
        if (oVRScreenFade)
        {
            oVRScreenFade.fadeTime = fadeTime;
            StartCoroutine(oVRScreenFade.Fade(1, 0));
        }
    }
}
