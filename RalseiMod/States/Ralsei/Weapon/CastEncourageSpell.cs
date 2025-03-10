﻿using EntityStates;
using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RalseiMod.States.Ralsei.Weapon
{
    class CastEncourageSpell : SpellBombBaseState
    {

        internal override float GetCastTime()
        {
            return HealSpell.minCastTime;
        }

        internal override float GetEffectRange()
        {
            return 15;
        }

        internal override float GetMaxDistance()
        {
            return 100;
        }

        internal override GameObject GetProjectilePrefab()
        {
            return Pacify.encourageProjectilePrefab; ;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}
