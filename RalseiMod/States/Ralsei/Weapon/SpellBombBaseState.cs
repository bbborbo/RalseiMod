﻿using EntityStates;
using EntityStates.Toolbot;
using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RalseiMod.States.Ralsei.Weapon
{
    class SpellBombBaseState : AimThrowableBase
    {
        Animator animator;
        public override void OnEnter()
        {
            base.detonationRadius = HealSpell.healRange;
            base.projectilePrefab = HealSpell.loveBomb;
            base.baseMinimumDuration = HealSpell.minCastTime;
            base.maxDistance = 100;
            base.setFuse = true;
            base.arcVisualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();
            base.endpointVisualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotMortarAreaIndicator.prefab").WaitForCompletion();
            base.OnEnter();

            if (this.endpointVisualizerTransform)
            {
                this.endpointVisualizerTransform.localScale = new Vector3(this.detonationRadius, this.detonationRadius, this.detonationRadius);
            }
            characterBody.AddBuff(RoR2Content.Buffs.Slow50);

            animator = GetModelAnimator();
            PlayAnimation("Gesture, Override", "PrepareSpellEntry", "SpellSecondary.playbackRate", this.minimumDuration);
            animator.SetBool("spellReady", true);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(3f);
        }
        public override void OnExit()
        {
            base.OnExit();
            animator.SetBool("spellReady", false);
            PlayAnimation("Gesture, Override", "CastSpellSecondary", "SpellSecondary.playbackRate", 1f / base.attackSpeedStat);
            if (characterBody.HasBuff(RoR2Content.Buffs.Slow50))
                characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
