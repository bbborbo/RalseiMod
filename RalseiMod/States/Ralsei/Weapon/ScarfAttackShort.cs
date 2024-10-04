using RalseiMod.Skills;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using R2API;
using RalseiMod.Survivors.Ralsei;

namespace RalseiMod.States.Ralsei.Weapon
{
    class ScarfAttackShort : ScarfAttackBaseState
    {
        public override bool isComboFinisher => step == ScarfShort.lastCombo;

        //these attacks choose the correct stat based on whether this state is determined to be the last in the combo
        public override float baseEnterDuration => isComboFinisher ? ScarfShort.comboEntryDuration : ScarfShort.baseEntryDuration;
        public override float baseExitDuration => isComboFinisher ? ScarfShort.comboExitDuration : ScarfShort.baseExitDuration;
        public override float damageCoefficient => isComboFinisher ? ScarfShort.baseDamageCombo : ScarfShort.baseDamage;
        public float procCoefficient => isComboFinisher ? ScarfShort.comboProcCoeff : ScarfShort.baseProcCoeff;
        public override float force => isComboFinisher ? ScarfShort.comboForce : ScarfShort.baseForce;
        public float hitstopDuration => isComboFinisher ? ScarfShort.comboHitstop : ScarfShort.baseHitstop;
        public float hitHopVelocity => isComboFinisher ? ScarfShort.comboHopStrength : ScarfShort.baseHopStrength;

        public override float altAnimationAttackSpeedThreshold => 2.65f;

        public override GameObject hitEffectPrefab => ScarfShort.slashImpactEffect;

        public override string GetAnimationLayer()
        {
            if (isComboFinisher)
                return "FullBody, Override";
            return base.GetAnimationLayer();
        }
        public override string GetAnimationName(int index)
        {
            if (characterBody.attackSpeed >= altAnimationAttackSpeedThreshold)
                return base.GetAnimationName(index % 2);

            if (isComboFinisher)
                return "PrimaryComboLong";

            return base.GetAnimationName(index);
        }
        public override string GetAttackSoundString()
        {
            if (isComboFinisher)
                return base.GetAttackSoundString();
            return base.GetAttackSoundString();
        }
        public override string GetHitSoundString()
        {
            if (isComboFinisher)
                return base.GetHitSoundString();
            return base.GetHitSoundString();
        }
        public override string GetMuzzleName()
        {
            if (isComboFinisher)
                return "SwingCenter";
            return (step % 2) == 0 ? "SwingR" : "SwingL";
        }

        public override void FireAttack()
        {
            FireOverlapAttack(GetOverlapAttack("ScarfGroup"), ScarfShort.slashEffectBasic);
        }

        public override void FireAttackCombo()
        {
            OverlapAttack attack = GetOverlapAttack("SpinGroup");
            attack.AddModdedDamageType(RalseiSurvivor.TangleOnHit);
            attack.AddModdedDamageType(Ror2AggroTools.AggroToolsPlugin.AggroOnHit);

            FireOverlapAttack(attack, ScarfShort.slashEffectCombo);
        }


        public override void OnEnter()
        {
            animator = GetModelAnimator();
            StartAimMode(0.5f + enterDuration + exitDuration, false);

            base.OnEnter();
        }
        public override void OnExit()
        {
            if (inHitPause)
            {
                RemoveHitstop();
            }
            base.OnExit();
        }

        OverlapAttack GetOverlapAttack(string hitboxGroupName)
        {
            OverlapAttack attack = new OverlapAttack();
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.damageType = DamageType.Generic;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = damageCoefficient * damageStat;
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = Vector3.zero;
            attack.pushAwayForce = force;
            attack.isCrit = RollCrit();
            attack.impactSound = NetworkSoundEventIndex.Invalid;
            return attack;
        }

        public void FireOverlapAttack(OverlapAttack overlapAttack, GameObject swingEffect)
        {
            EffectManager.SimpleMuzzleFlash(swingEffect, gameObject, GetMuzzleName(), false);
            if (isAuthority)
            {
                if (overlapAttack.Fire())
                {
                    OnHitEnemyAuthority();
                }
            }
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound(GetHitSoundString(), gameObject);

            if (!hasHopped)
            {
                if (characterMotor && !characterMotor.isGrounded && hitHopVelocity > 0f)
                {
                    SmallHop(characterMotor, hitHopVelocity);
                }

                hasHopped = true;
            }

            ApplyHitstop();
        }

        protected string playbackRateParam = "Slash.playbackRate";
        bool hasHopped = false;
        protected Animator animator;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        protected bool inHitPause;
        private float hitPauseTimer;
        protected void ApplyHitstop()
        {
            if (!inHitPause && hitstopDuration > 0f)
            {
                storedVelocity = characterMotor.velocity;
                hitStopCachedState = CreateHitStopCachedState(characterMotor, animator, playbackRateParam);
                hitPauseTimer = hitstopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }
        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;
        }
    }
}
