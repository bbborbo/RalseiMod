﻿using BepInEx.Configuration;
using EntityStates;
using R2API;
using RalseiMod.Modules;
using RalseiMod.States.Ralsei.Weapon;
using RalseiMod.Survivors.Ralsei;
using RalseiMod.Survivors.Ralsei.Components;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RalseiMod.Modules.Language.Styling;

namespace RalseiMod.Skills
{
    class HealSpell : SkillBase<HealSpell>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Ability Stock", 2)]
        public static int stock;

        [AutoConfig("Ability Cooldown", 14)]
        public static float cooldown;

        [AutoConfig("Heal Range", 35f)]
        public static float healRange;

        [AutoConfig("Minimum Cast Time", 0.4f)]
        public static float minCastTime;

        [AutoConfig("Immediate Heal Fraction", 0.1f)]
        public static float instantHealPercent;

        [AutoConfig("Delayed Heal Duration", 1f)]
        public static float healDuration;
        #endregion
        public static GameObject loveBomb;
        public static GameObject loveBombImpact;
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Heal Prayer";

        public override string SkillDescription => 
            $"Cast a {HealingColor("healing spell")} on yourself and all allies within {UtilityColor(healRange + "m")}, " +
            $"restoring {HealingColor(ConvertDecimal(instantHealPercent) + " health")} " +
            $"and granting {HealingColor("Regenerative")} for {HealingColor(healDuration.ToString())} seconds.";

        public override string SkillLangTokenName => "HEALSPELL";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Captain/CallSupplyDropHealing.asset");

        public override Type ActivationState => typeof(AimHealSpell);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Secondary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            baseRechargeInterval = cooldown,
            fullRestockOnAssign = false,
            beginSkillCooldownOnSkillEnd = true,
            isCombatSkill = false,
            canceledFromSprinting = true,
            cancelSprintingOnActivation = true,
            mustKeyPress = true,
            interruptPriority = InterruptPriority.Skill,
            baseMaxStock = stock
        };

        public override void Init()
        {
            KeywordTokens = new string[] { "KEYWORD_RAPID_REGEN" };
            base.Init();
            CreateBombProjectile();
            GameObject healexp = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthHealExplosion.prefab").WaitForCompletion();
            loveBombImpact = PrefabAPI.InstantiateClone(healexp, "RalseiLoveBombExplosion", false);
            loveBombImpact.transform.localScale *= (healRange / 12);
        }
        public override void Hooks()
        {

        }

        private static void CreateBombProjectile()
        {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            loveBomb = Modules.Assets.CloneProjectilePrefab("CryoCanisterProjectile", "RalseiLoveBomb");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(loveBomb.GetComponent<ProjectileImpactExplosion>());
            ProjectileHealOnImpact bombImpactExplosion = loveBomb.AddComponent<ProjectileHealOnImpact>();


            //bombImpactExplosion.blastRadius = 16f;
            //bombImpactExplosion.blastDamageCoefficient = 1f;
            //bombImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            //bombImpactExplosion.destroyOnEnemy = true;
            //bombImpactExplosion.lifetime = 12f;
            //bombImpactExplosion.impactEffect = Resources.Load<GameObject>("prefabs/effects/JellyfishNova");
            //bombImpactExplosion.lifetimeExpiredSound = Content.CreateAndAddNetworkSoundEventDef("HenryBombExplosion");
            //bombImpactExplosion.timerAfterImpact = true;
            //bombImpactExplosion.lifetimeAfterImpact = 0f;

            ProjectileController bombController = loveBomb.GetComponent<ProjectileController>();

            GameObject ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSpitGhost.prefab").WaitForCompletion();
            if (ghostPrefab/*_assetBundle.LoadAsset<GameObject>("HenryBombGhost")*/ != null)
                bombController.ghostPrefab = ghostPrefab;//Assets.CreateProjectileGhostPrefab("HenryBombGhost");

            bombController.startSound = "";
            Content.AddProjectilePrefab(loveBomb);
        }
    }
}
