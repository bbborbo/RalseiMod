using BepInEx.Configuration;
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
    class ProtectSpell : SkillBase<ProtectSpell>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Ability Stock", 1)]
        public static int stock;

        [AutoConfig("Ability Cooldown", 12)]
        public static float cooldown;

        [AutoConfig("Effect Range", 35f)]
        public static float effectRange;

        [AutoConfig("Minimum Cast Time", 0.4f)]
        public static float minCastTime;

        [AutoConfig("Block Chance", "Expressed as a fraction; ie, 0.3 means 30%", 0.3f)]
        public static float blockChance;

        [AutoConfig("Allow Block Buff Stacking", true)]
        public static bool blockStackable = true;

        [AutoConfig("Block Buff Duration", 6f)]
        public static float blockDuration;
        #endregion
        public static GameObject loveBomb;
        public static GameObject loveBombImpact;
        public static BuffDef blockBuff;
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Fluffy Guard";

        public override string SkillDescription => 
            $"Cast a {UtilityColor("protective spell")} on yourself and all allies within {UtilityColor(effectRange + "m")}, " +
            $"granting {UtilityColor(ConvertDecimal(blockChance) + " block chance")}" +
            $" for {UtilityColor(blockDuration.ToString())} seconds.";

        public override string SkillLangTokenName => "GUARDSPELL";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Engi/EngiBodyPlaceBubbleShield.asset");

        public override Type ActivationState => typeof(AimGuardSpell);

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
            CreateBlockBuff();
            GameObject healexp = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthHealExplosion.prefab").WaitForCompletion();
            loveBombImpact = PrefabAPI.InstantiateClone(healexp, "RalseiLoveBombExplosion", false);
            loveBombImpact.transform.localScale *= (effectRange / 12);
        }

        private void CreateBlockBuff()
        {
            blockBuff = Content.CreateAndAddBuff(
                "RalseiFluffyGuard", 
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffGenericShield.tif").WaitForCompletion(), 
                Color.grey,
                blockStackable, 
                false
                );
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += RalseiBlockBuff;
        }

        private void RalseiBlockBuff(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!damageInfo.damageType.damageType.HasFlag(DamageType.BypassBlock))
            {
                int buffCount = self.body.GetBuffCount(blockBuff);
                if (buffCount > 0)
                {
                    float endChance = 1 - Mathf.Pow(1 - blockChance, buffCount);
                    if(Util.CheckRoll(endChance * 100, 0f))
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
                        };
                        EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData, true);
                        damageInfo.rejected = true;
                    }
                }
            }
            orig(self, damageInfo);
        }

        private static void CreateBombProjectile()
        {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            loveBomb = Modules.Assets.CloneProjectilePrefab("CryoCanisterProjectile", "RalseiArmorBomb");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(loveBomb.GetComponent<ProjectileImpactExplosion>());
            ProjectileBuffOnImpact bombImpactExplosion = loveBomb.AddComponent<ProjectileBuffOnImpact>();


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
