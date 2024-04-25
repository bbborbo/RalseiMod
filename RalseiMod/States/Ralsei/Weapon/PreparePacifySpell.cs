using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei.Weapon
{
    class PreparePacifySpell : EmpowerSpellBaseState
    {
        public override float maxHealthFraction => 0.5f;

        public override bool useFriendlyTeam => false;

        public override GameObject indicatorPrefab => LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator");

        public override EntityState GetNextState()
        {
            return new CastPacifySpell
            {
                target = base.currentTarget
            };
        }
        public override void OnExit()
        {
            if (base.isAuthority)
                Log.Warning("PreparePacify authority, target real " + base.currentTarget != null);
            if (NetworkServer.active)
                Log.Warning("PreparePacify server, target real " + base.currentTarget != null);
            base.OnExit();
        }
    }
}
