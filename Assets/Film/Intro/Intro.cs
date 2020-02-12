using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Film
{
    public class Intro : MonoBehaviour
    {
        public SceneManager sceneManager;

        public void OnTitleFadeOut()
        {
            sceneManager.HandleTitleFadeOut();
        }
    }
}
