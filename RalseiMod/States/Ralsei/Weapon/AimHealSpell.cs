using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    class AimHealSpell : SpellBombBaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound("Play_gloom_effect_loop", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            Util.PlaySound("Stop_gloom_effect_loop", gameObject);
        }

        internal override float GetCastTime()
        {
            return HealSpell.minCastTime;
        }

        internal override float GetEffectRange()
        {
            return HealSpell.healRange;
        }

        internal override float GetMaxDistance()
        {
            return 100;
        }

        internal override GameObject GetProjectilePrefab()
        {
            return HealSpell.loveBomb;
        }
    }
}
