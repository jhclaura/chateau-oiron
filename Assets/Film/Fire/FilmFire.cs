using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Film
{
    public class FilmFire : MonoBehaviour
    {
        public Mascot mascot;

        public void ActivateMascot()
        {
            mascot.mascotAnimator.SetTrigger("StartWalking");
            mascot.mascotAnimator.SetFloat("LegMove", 1f);
        }

        public void MascotContinueWalking()
        {
            mascot.TriggerAnimation("ContinueWalking");
        }
    }
}