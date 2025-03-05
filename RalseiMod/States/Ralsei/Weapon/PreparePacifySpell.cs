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

        public override void OnEnter()
        {
            base.OnEnter();
            base.SetScopeAlpha(0f);
        }
        public override void Update()
        {
            base.Update();
            base.SetScopeAlpha(Mathf.Clamp01(base.age / 2f));
        }

        public override EntityState GetNextState()
        {
            return new CastPacifySpell
            {
                target = base.currentTarget
            };
        }
    }
}
