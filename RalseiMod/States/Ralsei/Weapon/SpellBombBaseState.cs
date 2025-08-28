using EntityStates;
using EntityStates.Toolbot;
using RalseiMod.Skills;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RalseiMod.Survivors.Ralsei.Components;
using RalseiMod.Modules;

namespace RalseiMod.States.Ralsei.Weapon
{
    public abstract class SpellBombBaseState : AimThrowableBase
    {
        public GameObject spellLightingEffectInstance;

        Animator animator;
        internal abstract float GetEffectRange();
        internal abstract GameObject GetProjectilePrefab();
        internal abstract float GetCastTime();
        internal abstract float GetMaxDistance();

        internal bool teleportAllies = false;
        public override void OnEnter()
        {
            //this.spellLightingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(RalseiSurvivor.ralseiSpellPrepareEffect, this.transform);

            base.detonationRadius = GetEffectRange();
            base.projectilePrefab = GetProjectilePrefab();
            base.baseMinimumDuration = GetCastTime();
            base.maxDistance = GetMaxDistance();
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

            if(base.inputBank.skill2.down && fixedAge > HealSpell.teleportTime)
            {
                TeleportAllies();

                outer.SetNextStateToMain();
            }
        }

        public void TeleportAllies()
        {
            teleportAllies = true;

            List<RalseiRecallComponent> allyList = RalseiRecallComponent.GetAllWarpTargets();

            Vector3 pos = transform.position;
            int i = 0;
            int count = allyList.Count;

            foreach (RalseiRecallComponent ally in allyList)
            {
                if (ally)
                {
                    CharacterBody allyBody = ally.master.GetBody();

                    // 7.5 is the magic number to have all turrets on the teleporter platform
                    // needs to be slightly larger for the primordial telepot
                    float Radius = 25f;
                    float radianInc = Mathf.Deg2Rad * 360f / count;
                    Vector3 point1 = new Vector3(Mathf.Cos(radianInc * i) * Radius, 0.25f, Mathf.Sin(radianInc * i) * Radius);

                    float flatIncreasePerLevel = 15 * characterBody.level;
                    float healPerStack = (allyBody.maxHealth * 0.1f) + (20f + flatIncreasePerLevel);

                    float totalHeal = healPerStack * activatorSkillSlot.stock;

                    allyBody.healthComponent.Heal(totalHeal, new ProcChainMask(), true);

                    i++;

                    var targetFootPos = pos + point1;

                    TeleportHelper.TeleportBody(allyBody, targetFootPos);
                }
            }

            activatorSkillSlot.stock = 0;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (spellLightingEffectInstance)
                //EntityState.Destroy(spellLightingEffectInstance);

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