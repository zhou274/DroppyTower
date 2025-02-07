using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DroppyTower
{
    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;
        [HideInInspector]
        public int simultaneousPlayCount = 0;
    }
}