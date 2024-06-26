﻿using RalseiMod.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using R2API;
using RalseiMod.Survivors.Ralsei;
using EntityStates.Loader;

namespace RalseiMod.States.Ralsei.Weapon
{
    class ScarfAttackLong : ScarfAttackBaseState
    {
        public override bool isComboFinisher => step == ScarfRange.lastCombo;

        //these attacks choose the correct stat based on whether this state is determined to be the last in the combo
        public override float baseEnterDuration => isComboFinisher ? ScarfRange.comboEntryDuration : ScarfRange.baseEntryDuration;
        public override float baseExitDuration => isComboFinisher ? ScarfRange.comboExitDuration : ScarfRange.baseExitDuration;
        public override float damageCoefficient => isComboFinisher ? ScarfRange.baseDamageCombo : ScarfRange.baseDamage;

        public override float altAnimationAttackSpeedThreshold => 2.65f;

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
        public override string GetMuzzleName()
        {
            if (characterBody.attackSpeed >= altAnimationAttackSpeedThreshold)
                return base.GetMuzzleName();

            if (step == ScarfRange.lastCombo)
                return "SwingSpin";
            if (step == ScarfRange.lastCombo - 1)
                return "SwingJab";

            return base.GetMuzzleName();
        }

        //fire attack combo and regular are split because some applications of the base state would want to do two different kinds of attacks
        //however in this state, both attacks are essentially identical aside from their AVFX, so they are using a common BulletAttack 
        public override void FireAttackCombo()
        {
            float recoil = 2.8f / this.attackSpeedStat;
            base.AddRecoil(-recoil, -2f * recoil, -recoil, recoil);
            base.characterBody.SetAimTimer(2f);
            Util.PlaySound(new LoaderMeleeAttack().beginSwingSoundString, base.gameObject);

            if (base.isAuthority)
            {
                BulletAttack ba = GetBulletAttack();
                //ba.damageType = DamageType.CrippleOnHit;
                ba.AddModdedDamageType(RalseiSurvivor.TangleOnHit);
                ba.AddModdedDamageType(Ror2AggroTools.AggroToolsPlugin.AggroOnHit);
                ba.Fire();
            }
        }
        public override void FireAttack()
        {
            float recoil = 1.2f / this.attackSpeedStat;
            base.AddRecoil(-recoil, -2f * recoil, -recoil, recoil);
            base.characterBody.SetAimTimer(2f);
            Util.PlaySound(new LoaderMeleeAttack().beginSwingSoundString, base.gameObject);

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
            bulletAttack.tracerEffectPrefab = isComboFinisher ? ScarfRange.tracerThreadCombo : ScarfRange.tracerThread;
            bulletAttack.muzzleName = this.muzzleString;
            bulletAttack.hitEffectPrefab = ScarfRange.tracerImpact;
            bulletAttack.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
            bulletAttack.radius = 1.2f;
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.stopperMask = LayerIndex.world.mask;
            //maxDistance = maxRange;
            bulletAttack.smartCollision = true;

            return bulletAttack;
        }
    }
}
