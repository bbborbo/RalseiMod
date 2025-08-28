using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    class AimGuardSpell : SpellBombBaseState
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
            return ProtectSpell.minCastTime;
        }

        internal override float GetEffectRange()
        {
            return ProtectSpell.effectRange;
        }

        internal override float GetMaxDistance()
        {
            return 100;
        }

        internal override GameObject GetProjectilePrefab()
        {
            return ProtectSpell.loveBomb;
        }
    }
}
