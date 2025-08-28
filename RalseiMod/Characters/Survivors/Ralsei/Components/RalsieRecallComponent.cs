using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.Survivors.Ralsei.Components
{
    [RequireComponent(typeof(CharacterMaster))]
    public class RalseiRecallComponent : MonoBehaviour
    {
        static List<RalseiRecallComponent> allWarpTargets = new List<RalseiRecallComponent>();

        public static RalseiRecallComponent[] GetWarpTargets(TeleporterInteraction tp)
        {
            return GetWarpTargets(tp.transform.position, tp.holdoutZoneController.baseRadius);
        }

        public static List<RalseiRecallComponent> GetAllWarpTargets()
        {
            return allWarpTargets;
        }

        public static RalseiRecallComponent[] GetWarpTargets(Vector3 startPosition, float radius)
        {
            List<RalseiRecallComponent> filteredWarpTargets = new List<RalseiRecallComponent>();
            foreach (RalseiRecallComponent warpTarget in allWarpTargets)
            {
                if (warpTarget.master && warpTarget.master.teamIndex == TeamIndex.Player)
                {
                    CharacterBody b = warpTarget.master.GetBody();

                    if (b != null && (radius == 0 || (startPosition - b.corePosition).sqrMagnitude >= radius * radius))
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
