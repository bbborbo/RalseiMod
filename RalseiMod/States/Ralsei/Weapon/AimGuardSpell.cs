using RalseiMod.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    class AimGuardSpell : SpellBombBaseState
    {
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
