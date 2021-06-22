using System;
using UnityEngine;

namespace Player.Weapon
{
    [Serializable]
    public class Impact
    {
        public string tag;
        public GameObject effect;
        public float destroyAfter = 0.2f;
    }
}