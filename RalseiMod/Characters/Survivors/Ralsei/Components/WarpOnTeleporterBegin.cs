﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.Survivors.Ralsei.Components
{
    [RequireComponent(typeof(CharacterMaster))]
    class WarpOnTeleporterBegin : MonoBehaviour
    {
        public static List<WarpOnTeleporterBegin> allWarpTargets = new List<WarpOnTeleporterBegin>();

        public static WarpOnTeleporterBegin[] GetWarpTargets(TeleporterInteraction tp)
        {
            List<WarpOnTeleporterBegin> filteredWarpTargets = new List<WarpOnTeleporterBegin>();
            foreach (WarpOnTeleporterBegin warpTarget in allWarpTargets)
            {
                CharacterBody b = warpTarget.master.GetBody();
                if (b != null && !tp.holdoutZoneController.IsBodyInChargingRadius(b))
                    if (warpTarget.master.teamIndex == TeamIndex.Player)
                        filteredWarpTargets.Add(warpTarget);
            }
            return filteredWarpTargets.ToArray();
        }

        public CharacterMaster master;
        void Start()
        {
            if (master == null)
                master = GetComponent<CharacterMaster>();
            if (master == null)
                Destroy(this);

            allWarpTargets.Add(this);
        }
        void OnDestroy()
        {
            allWarpTargets.Remove(this);
        }
    }
}