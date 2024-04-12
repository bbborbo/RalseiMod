using EntityStates;
using EntityStates.Toolbot;
using RalseiMod.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RalseiMod.States.Ralsei.Weapon
{
    class SpellBombBaseState : AimThrowableBase
    {
        public override void OnEnter()
        {
            base.detonationRadius = HealSpell.healRange;
            base.projectilePrefab = HealSpell.loveBomb;
            base.baseMinimumDuration = 0.2f;
            base.maxDistance = 100;
            base.setFuse = true;
            base.arcVisualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();
            base.endpointVisualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();
            base.OnEnter();
            if (this.endpointVisualizerTransform)
            {
                this.endpointVisualizerTransform.localScale = new Vector3(this.detonationRadius, this.detonationRadius, this.detonationRadius);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
