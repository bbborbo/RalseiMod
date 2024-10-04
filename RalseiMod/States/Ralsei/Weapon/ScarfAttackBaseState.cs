using EntityStates;
using EntityStates.Loader;
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
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(step);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            step = reader.ReadInt32();
        }

        public abstract float altAnimationAttackSpeedThreshold { get; }
        public abstract float baseEnterDuration { get; }
        public abstract float baseExitDuration { get; }
        private protected float enterDuration;
        private protected float exitDuration;
        float totalDuration => enterDuration + exitDuration;
        float stopwatch;
        bool fired = false;
        internal string muzzleString;

        public abstract float damageCoefficient { get; }
        public virtual float force { get; } = 0;
        public static float spreadBloomValue = 0.2f;

        public abstract GameObject hitEffectPrefab { get; }
        public override void OnEnter()
        {
            base.OnEnter();

            exitDuration = baseExitDuration / attackSpeedStat;
            this.muzzleString = GetMuzzleName();

            if (baseEnterDuration > 0)
            {
                enterDuration = baseEnterDuration / attackSpeedStat;
            }
            else
            {
                TryAttack();
            }

            PlayCrossfade(GetAnimationLayer(), GetAnimationName(this.step), "ScarfPrimary.playbackRate",
                totalDuration * 1.5f, 0.1f * totalDuration);
        }
        public virtual string GetAnimationLayer()
        {
            return "Gesture, Override";
        }
        public virtual string GetAnimationName(int index)
        {
            return "Primary" + (index + 1);
        }
        public virtual string GetMuzzleName()
        {
            return "SwingCenter";
        }
        public virtual string GetAttackSoundString()
        {
            return new LoaderMeleeAttack().beginSwingSoundString;// "HenrySwordSwing";
        }
        public virtual string GetHitSoundString()
        {
            return "HenrySwordSwing";
        }
        public override void OnExit()
        {
            base.OnExit();
            {
                if (!fired)
                    TryAttack();
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(base.fixedAge >= enterDuration && !fired)
            {
                TryAttack();
            }
            if(base.fixedAge >= totalDuration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public virtual void TryAttack()
        {
            if (fired)
                return;

            fired = true;
            Util.PlaySound(GetAttackSoundString(), base.gameObject);

            if (isComboFinisher)
            {
                FireAttackCombo();
                return;
            }
            FireAttack();
        }
        public abstract void FireAttackCombo();
        public abstract void FireAttack();

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
