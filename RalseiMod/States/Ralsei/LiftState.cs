using EntityStates;
using EntityStates.VoidSurvivor;
using RalseiMod.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei
{
    class LiftState : GenericCharacterMain
    {
        float liftRate;
        float duration;
        public override void OnEnter()
        {
            liftRate = LiftPrayer.liftSpeed * this.attackSpeedStat;
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
            AnimationCurve upSpeed = AnimationCurve.EaseInOut(0, 0.2f, 1, 1);
            float time = base.fixedAge / this.duration;
            float b = /*new VoidBlinkBase().*/upSpeed.Evaluate(time);
            return b * this.liftRate * this.moveSpeedStat;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
