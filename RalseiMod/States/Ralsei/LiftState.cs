using EntityStates;
using EntityStates.VoidSurvivor;
using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei
{
    class LiftState : GenericCharacterMain
    {
        static bool useMoveSpeed => LiftPrayer.useMoveSpeed;
        float liftRateStart;
        float liftRateMin;
        float liftRateMax;
        float duration;
        public override void OnEnter()
        {
            liftRateMin = LiftPrayer.liftSpeedMin * this.attackSpeedStat;
            liftRateMax = LiftPrayer.liftSpeedMax * this.attackSpeedStat;
            liftRateStart = liftRateMax > 0 ? liftRateMin / liftRateMax : 0;
            duration = LiftPrayer.liftDuration / this.attackSpeedStat;
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterMotor && base.characterDirection)
            {
                if (base.characterMotor)
                {
                    base.characterMotor.Motor.ForceUnground();
                }
                float velocity = this.GetLiftVelocity();
                base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, velocity, base.characterMotor.velocity.z);
                /*if (this.blinkVfxInstance)
                {
                    this.blinkVfxInstance.transform.forward = velocity;
                }*/
            }
            if (base.fixedAge >= LiftPrayer.liftDuration)
            {
                this.outer.SetNextState(new HoverState());
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        private float GetLiftVelocity()
        {
            AnimationCurve upSpeed = AnimationCurve.EaseInOut(0, liftRateStart, 1, 1);

            float time = base.fixedAge / this.duration;
            float b = Util.Remap(upSpeed.Evaluate(time), liftRateStart, 1, liftRateMin, liftRateMax);
            if (useMoveSpeed)
                b *= this.moveSpeedStat;
            return b;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
