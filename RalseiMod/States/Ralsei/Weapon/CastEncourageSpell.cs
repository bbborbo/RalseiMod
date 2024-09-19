using EntityStates;
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
        public override void OnEnter()
        {
            base.detonationRadius = 15;
            base.projectilePrefab = Pacify.encourageProjectilePrefab;
            base.OnEnter();
            base.detonationRadius = 15;
            base.projectilePrefab = Pacify.encourageProjectilePrefab;
        }
    }
}
