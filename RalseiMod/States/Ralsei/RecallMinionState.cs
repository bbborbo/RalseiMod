using EntityStates;
using RalseiMod.Survivors.Ralsei.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei
{
    class RecallMinionState : EntityState
    {
        public override void OnEnter()
        {
            if(NetworkServer.active)
                TeleportAllies();
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            outer.SetNextStateToMain();
        }

        public void TeleportAllies()
        {
            List<RalseiRecallComponent> allyList = RalseiRecallComponent.GetAllWarpTargets();

            Vector3 pos = transform.position;
            int i = 0;
            int count = allyList.Count;

            foreach (RalseiRecallComponent ally in allyList)
            {
                if (ally)
                {
                    CharacterBody allyBody = ally.master.GetBody();

                    // 7.5 is the magic number to have all turrets on the teleporter platform
                    // needs to be slightly larger for the primordial telepot
                    float Radius = 25f;
                    float radianInc = Mathf.Deg2Rad * 360f / count;
                    Vector3 point1 = new Vector3(Mathf.Cos(radianInc * i) * Radius, 0.25f, Mathf.Sin(radianInc * i) * Radius);

                    float flatIncreasePerLevel = 15 * characterBody.level;
                    float healPerStack = (allyBody.maxHealth * 0.1f) + (20f + flatIncreasePerLevel);

                    float totalHeal = healPerStack * skillLocator.secondary.stock;

                    allyBody.healthComponent.Heal(totalHeal, new ProcChainMask(), true);

                    i++;

                    var targetFootPos = pos + point1;

                    TeleportHelper.TeleportBody(allyBody, targetFootPos);
                }
            }

            skillLocator.secondary.stock = 0;
        }
    }
}
