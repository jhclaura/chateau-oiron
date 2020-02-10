using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Film
{
    public class Intro : MonoBehaviour
    {
        public SceneManager sceneManager;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnTitleFadeOut()
        {
            sceneManager.HandleTitleFadeOut();
        }
    }
}
