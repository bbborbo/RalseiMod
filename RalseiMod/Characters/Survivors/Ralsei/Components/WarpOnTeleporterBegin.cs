using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.Survivors.Ralsei.Components
{
    [RequireComponent(typeof(CharacterMaster))]
    class WarpOnTeleporterBegin : MonoBehaviour
    {
        public CharacterMaster master;
        void Start()
        {
            if (master == null)
                master = GetComponent<CharacterMaster>();
            if (master == null)
                Destroy(this);
        }
    }
}
