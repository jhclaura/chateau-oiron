using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Film
{
    public class FilmWater : MonoBehaviour
    {
        public void DisableFog()
        {
            RenderSettings.fog = false;
        }

        public void EnableFog()
        {
            RenderSettings.fog = true;
        }
    }
}