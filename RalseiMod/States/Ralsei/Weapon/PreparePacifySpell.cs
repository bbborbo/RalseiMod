using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    class PreparePacifySpell : EmpowerSpellBaseState
    {
        public override float maxHealthFraction => 0.5f;

        public override bool useFriendlyTeam => false;

        public override GameObject indicatorPrefab => LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator");

        public override EntityState GetNextState()
        {
            return new CastPacifySpell()
            {
                target = base.currentTarget
            };
        }
    }
}
