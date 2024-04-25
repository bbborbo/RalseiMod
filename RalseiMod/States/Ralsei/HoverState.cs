using EntityStates;
using EntityStates.Mage;
using RalseiMod.Skills;
using RoR2;
using Ror2AggroTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei
{
    class HoverState : GenericCharacterMain
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Aggro.ShedAggroFromCharacter(characterBody);
            base.skillLocator.utility.SetSkillOverride(this, CancelHoverSkill.instance.SkillDef, RoR2.GenericSkill.SkillOverridePriority.Contextual);
            if (base.isAuthority)
            {
                characterBody.AddBuff(LiftPrayer.hoverBuff);
                base.characterMotor.onHitGroundAuthority += this.CharacterMotor_onHitGroundAuthority;
            }
        }

        private void CharacterMotor_onHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            this.outer.SetNextStateToMain();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                //cancel hover mode with skill press
                if (base.inputBank.skill3.justReleased)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }

                float num = base.characterMotor.velocity.y;
                if(num < 0)
                {
                    num = Mathf.MoveTowards(num, LiftPrayer.hoverVelocity, LiftPrayer.hoverAcceleration * Time.fixedDeltaTime);
                    base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, num, base.characterMotor.velocity.z);
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            base.skillLocator.utility.UnsetSkillOverride(this, CancelHoverSkill.instance.SkillDef, RoR2.GenericSkill.SkillOverridePriority.Contextual);
            if (base.isAuthority)
            {
                characterBody.RemoveBuff(LiftPrayer.hoverBuff);
                base.characterMotor.onHitGroundAuthority -= this.CharacterMotor_onHitGroundAuthority;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
