using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public enum EnvironmentType
{
    Water,
    Fire,
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
    public EnvironmentSettings environmentSettings;
    public Material skyboxMaterial;
	//public EnvironmentColor[] environmentColors;

    private bool isLoading;
    private OVRScreenFade oVRScreenFade;
    private Dictionary<EnvironmentType, Environment> environmentDictionary;
    //private Dictionary<EnvironmentType, string> environmentSceneNameDictionary;
    //private Dictionary<string, EnvironmentType> environmentTypeDictionary;
    //private Dictionary<EnvironmentType, EnvironmentColor> environmentColorDictionary;

    void Awake()
    {
        environmentDictionary = new Dictionary<EnvironmentType, Environment>();
        foreach(Environment env in environmentSettings.environmentOrder)
        {
            environmentDictionary.Add(env.environmentType, env);
        }

        //environmentSceneNameDictionary = new Dictionary<EnvironmentType, string>()
        //{
        //    { EnvironmentType.Fire, "Scene_fire" },
        //    { EnvironmentType.Water, "Scene_water" },
        //    { EnvironmentType.Forest, "Scene_forest" },
        //    { EnvironmentType.Beetle, "Scene_beetle" }
        //};

        //environmentTypeDictionary = new Dictionary<string, EnvironmentType>()
        //{
        //    { "fire", EnvironmentType.Fire },
        //    { "water", EnvironmentType.Water },
        //    { "forest", EnvironmentType.Forest },
        //    { "beetle", EnvironmentType.Beetle }
        //};

        //environmentColorDictionary = new Dictionary<EnvironmentType, EnvironmentColor>()
        //{
        //    { EnvironmentType.Fire, environmentColors[0] },
        //    { EnvironmentType.Water, environmentColors[1] },
        //    { EnvironmentType.Forest, environmentColors[2] },
        //    { EnvironmentType.Beetle, environmentColors[3] }
        //};

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
        SceneManager.LoadSceneAsync(environmentDictionary[currentEnvironment].sceneName, LoadSceneMode.Additive);
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

    //public void HandleEnvironmentChange(string newEnv)
    //{
    //    HandleEnvironmentChange(environmentTypeDictionary[newEnv]);
    //}

    IEnumerator LoadEnvironmentSceneAsync(EnvironmentType newEnv)
    {
        FadeOut(environmentDictionary[currentEnvironment].cameraFadeOutColor);
        yield return new WaitForSeconds(1f);

        // Unload
        if (environmentDictionary.ContainsKey(currentEnvironment))
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(environmentDictionary[currentEnvironment].sceneName);
            // wait until the async scene fully unloads
            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        // Load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(environmentDictionary[newEnv].sceneName, LoadSceneMode.Additive);
        currentEnvironment = newEnv;

        // wait until the async scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Call event
        

        // change env colors
        UpdateEnvironmentColors();

        // Scene load! fade in camera view
        yield return new WaitForSeconds(0.5f);
        FadeIn();

        isLoading = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "_BaseScene") return;
        string envName = currentEnvironment.ToString();       
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

        Environment env = environmentDictionary[currentEnvironment];

        skyboxMaterial.SetColor("_Color2", env.skyColors[0]);
        skyboxMaterial.SetColor("_Color1", env.skyColors[1]);
        RenderSettings.ambientSkyColor = env.ambientColors[0];
        RenderSettings.ambientEquatorColor = env.ambientColors[0];
        RenderSettings.ambientGroundColor = env.ambientColors[0];

    }

    //public string GetEnvrionmentName(EnvironmentType env)
    //{
    //    if (environmentSceneNameDictionary.TryGetValue(env, out string envName))
    //    {
    //        return envName;
    //    }
    //    else
    //    {
    //        return "";
    //    }
    //}

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
