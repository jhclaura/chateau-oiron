using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Film
{
    public class SceneManager : MonoBehaviour
    {
        public EnvironmentType currentEnvironment;
        public ChateauScene currentScene;

        [Header("Intro")]
        public Monologue introMonologue;
        public GameObject introWall;
        public GameObject introWallMask;
        public Animator titleAnimator;
        private Material introWallMaterial;

        private void Awake()
        {
            switch (currentEnvironment)
            {
                case EnvironmentType.Intro:
                    introWallMaterial = introWall.GetComponent<Renderer>().sharedMaterial;
                    introWallMaterial.color = Color.black;
                    introWall.SetActive(false);
                    introWallMask.SetActive(false);
                    break;

                case EnvironmentType.Water:
                    break;

                case EnvironmentType.Fire:
                    break;

                case EnvironmentType.Forest:
                    break;

                case EnvironmentType.Beetle:
                    break;
            }
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);

            switch (currentEnvironment)
            {
                case EnvironmentType.Intro:
                    StartIntro();
                    break;

                case EnvironmentType.Water:
                    currentScene.ActivateScene();
                    break;

                case EnvironmentType.Fire:
                    currentScene.ActivateScene();
                    break;

                case EnvironmentType.Forest:
                    currentScene.ActivateScene();
                    break;

                case EnvironmentType.Beetle:
                    currentScene.ActivateScene();
                    break;
            }
        }

        private void StartIntro()
        {
            // Start intro sound
            MonologueManager.Instance.Play(introMonologue);
            
            titleAnimator.SetTrigger("IntroFadeIn");
        }

        public void HandleTitleFadeOut()
        {
            introWall.SetActive(true);
            LeanTween.value(introWall, 0f, 1f, 3f)
                    .setEaseInOutQuad()
                    .setOnUpdate((float val) =>
                    {
                        introWallMaterial.color = Color.Lerp(introWallMaterial.color, Color.white, val);
                    })
                    .setOnComplete(()=> {
                        introWallMask.SetActive(true);
                    });
        }
    }
}
