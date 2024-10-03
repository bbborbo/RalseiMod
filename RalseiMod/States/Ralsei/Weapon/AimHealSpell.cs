using RalseiMod.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    class AimHealSpell : SpellBombBaseState
    {
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
