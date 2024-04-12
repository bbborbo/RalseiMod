using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    class ScarfAttackLong : ScarfAttackBaseState
    {
        public override bool isComboFinisher => step == ScarfRange.lastCombo;

        //these attacks choose the correct stat based on whether this state is determined to be the last in the combo
        public override float baseEnterDuration => isComboFinisher ? ScarfRange.comboEntryDuration : ScarfRange.baseEntryDuration;
        public override float baseExitDuration => isComboFinisher ? ScarfRange.comboExitDuration : ScarfRange.baseExitDuration;
        public override float damageCoefficient => isComboFinisher ? ScarfRange.baseDamageCombo : ScarfRange.baseDamage;



        //fire attack combo and regular are split because some applications of the base state would want to do two different kinds of attacks
        //however in this state, both attacks are essentially identical aside from their AVFX, so they are using a common BulletAttack 
        public override void FireAttackCombo()
        {
            float recoil = 5.5f / this.attackSpeedStat;
            base.AddRecoil(-recoil, -2f * recoil, -recoil, recoil);
            base.characterBody.SetAimTimer(2f);

            if (base.isAuthority)
            {
                BulletAttack ba = GetBulletAttack();
                ba.damageType = DamageType.WeakOnHit;
                ba.Fire();
            }
        }
        public override void FireAttack(Hand hand)
        {
            float recoil = 3.2f / this.attackSpeedStat;
            base.AddRecoil(-recoil, -2f * recoil, -recoil, recoil);
            base.characterBody.SetAimTimer(2f);

            if (base.isAuthority)
            {
                GetBulletAttack().Fire();
            }
        }
        BulletAttack GetBulletAttack()
        {
            Ray aimRay = base.GetAimRay();

            BulletAttack bulletAttack = new BulletAttack();
            bulletAttack.owner = base.gameObject;
            bulletAttack.weapon = base.gameObject;
            bulletAttack.origin = aimRay.origin;
            bulletAttack.aimVector = aimRay.direction;
            bulletAttack.minSpread = 0f;
            bulletAttack.maxSpread = 0f;
            bulletAttack.damage = damageCoefficient * this.damageStat;
            bulletAttack.force = force;
            bulletAttack.tracerEffectPrefab = ScarfRange.tracerThread;
            bulletAttack.muzzleName = this.muzzleString;
            bulletAttack.hitEffectPrefab = ScarfRange.tracerImpact;
            bulletAttack.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
            bulletAttack.radius = 0.8f;
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            //maxDistance = maxRange;
            bulletAttack.smartCollision = true;

            return bulletAttack;
        }
    }
}
