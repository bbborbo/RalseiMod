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
        static List<WarpOnTeleporterBegin> allWarpTargets = new List<WarpOnTeleporterBegin>();

        public static WarpOnTeleporterBegin[] GetWarpTargets(TeleporterInteraction tp)
        {
            return GetWarpTargets(tp.transform.position, tp.holdoutZoneController.baseRadius);
        }
        public static WarpOnTeleporterBegin[] GetWarpTargets(Vector3 startPosition, float radius)
        {
            List<WarpOnTeleporterBegin> filteredWarpTargets = new List<WarpOnTeleporterBegin>();
            foreach (WarpOnTeleporterBegin warpTarget in allWarpTargets)
            {
                if (warpTarget.master && warpTarget.master.teamIndex == TeamIndex.Player)
                {
                    CharacterBody b = warpTarget.master.GetBody();

                    if (b != null && (radius == 0 || (startPosition - b.corePosition).sqrMagnitude <= radius * radius))
                    {
                        filteredWarpTargets.Add(warpTarget);
                    }
                }
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
