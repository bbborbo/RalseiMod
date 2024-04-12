using EntityStates;
using RalseiMod.Skills;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei.Weapon
{
    abstract class ScarfAttackBaseState : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        internal int step;
        public void SetStep(int i)
        {
            step = i;
        }
        public abstract bool isComboFinisher { get; }
        private Hand hand => (Hand)(this.step % 2);
        public enum Hand
        {
            Left,
            Right
        }

        public abstract float baseEnterDuration { get; }
        public abstract float baseExitDuration { get; }
        float enterDuration;
        float exitDuration;
        float stopwatch;
        bool fired = false;
        internal string muzzleString = "MuzzleScarf";

        public abstract float damageCoefficient { get; }
        public static float force = 0;
        public static float spreadBloomValue = 0.2f;
        public static float maxRange = 100f;

        public static GameObject hitEffectPrefab;
        public override void OnEnter()
        {
            base.OnEnter();

            exitDuration = baseExitDuration / attackSpeedStat;

            if(baseEnterDuration > 0)
            {
                enterDuration = baseEnterDuration / attackSpeedStat;
            }
            else
            {
                TryAttack();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(base.fixedAge >= enterDuration && !fired)
            {
                TryAttack();
            }
            if(base.fixedAge >= exitDuration + enterDuration)
            {
                if (!fired)
                    TryAttack();
                this.outer.SetNextStateToMain();
            }
        }

        public virtual void TryAttack()
        {
            if (fired)
                return;

            fired = true;
            if (isComboFinisher)
            {
                FireAttackCombo();
                return;
            }
            FireAttack(hand);

            if (base.isAuthority)
            {
                if (!characterMotor.isGrounded)
                    characterMotor.velocity.y = Mathf.Min(characterMotor.velocity.y + 7, 3);
                    //base.SmallHop(characterMotor, 7/* / this.attackSpeedStat*/);
            }
        }
        public abstract void FireAttackCombo();
        public abstract void FireAttack(Hand hand);

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
