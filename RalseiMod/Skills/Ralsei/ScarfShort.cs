using EntityStates;
using R2API;
using RalseiMod.Modules;
using RalseiMod.States.Ralsei.Weapon;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RalseiMod.Modules.Language.Styling;

namespace RalseiMod.Skills
{
    class ScarfShort : SkillBase<ScarfShort>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Damage : Base Attack Damage", 1.8f)]
        public static float baseDamage;
        [AutoConfig("Damage : Combo Attack Base Damage", 4f)]
        public static float baseDamageCombo;
        [AutoConfig("Damage : Base Attack Proc Coefficient", 0.8f)]
        public static float baseProcCoeff;
        [AutoConfig("Damage : Combo Attack Proc Coefficient", "Proc coefficient will affect the duration of the Tangled status effect", 1f)]
        public static float comboProcCoeff;
        [AutoConfig("Damage : Base Attack Force", 300f)]
        public static float baseForce;
        [AutoConfig("Damage : Combo Attack Force", 750f)]
        public static float comboForce;
        [AutoConfig("Damage : Base Attack Hitstop Duration", 0.00f)]
        public static float baseHitstop;
        [AutoConfig("Damage : Combo Attack Hitstop Duration", 0.00f)]
        public static float comboHitstop;
        [AutoConfig("Damage : Base Attack Hop Velocity", 4f)]
        public static float baseHopStrength;
        [AutoConfig("Damage : Combo Attack Hop Velocity", 9f)]
        public static float comboHopStrength;

        public static int comboCount = 4;
        [AutoConfig("Combo : Grace Duration", "The time in seconds that Thread Slash should wait after attacking for a new input that continues the combo. Min of 0.02", 0.1f)]
        public static float comboGraceDuration;

        [AutoConfig("Duration : Base Attack Entry Duration", 0.1f)]
        public static float baseEntryDuration;
        [AutoConfig("Duration : Base Attack Exit Duration", 0.26f)]
        public static float baseExitDuration;
        [AutoConfig("Duration : Combo Attack Entry Duration", 0.4f)]
        public static float comboEntryDuration;
        [AutoConfig("Duration : Combo Attack Exit Duration", 0.7f)]
        public static float comboExitDuration;
        #endregion
        internal static int lastCombo => comboCount - 1;
        public static GameObject slashEffectBasic;
        public static GameObject slashEffectCombo;
        public static GameObject slashImpactEffect => ScarfRange.threadImpact;

        public override string SkillName => "Thread Slash";

        public override string SkillDescription => $"Slash your scarf at nearby enemies for {DamageValueText(baseDamage)}. " +
            $"Every {DamageColor(NumToAdj(comboCount))} attack spins around, " +
            $"{UtilityColor("Tangling")} enemies for {DamageValueText(baseDamageCombo)}.";

        public override string SkillLangTokenName => "SCARFSHORT";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Merc/MercBodyWhirlwind.asset");

        public override Type ActivationState => typeof(ScarfAttackShort);

        public override Type BaseSkillDef => typeof(SteppedSkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Primary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 0,
            interruptPriority = InterruptPriority.Any
        };
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override void Hooks()
        {

        }
        public override void Init()
        {
            KeywordTokens = new string[] { RalseiSurvivor.tangleKeywordToken };
            base.Init();
            (SkillDef as SteppedSkillDef).stepCount = 4;// Mathf.Max(comboCount, 1);
            (SkillDef as SteppedSkillDef).stepGraceDuration = Mathf.Max(comboGraceDuration, 0.02f);

            CreateSlashEffects();
        }

        private void CreateSlashEffects()
        {
            slashEffectBasic = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("RalseiScarfSlash");
            EffectComponent ec1 = slashEffectBasic.AddComponent<EffectComponent>();
            ec1.parentToReferencedTransform = true;
            ec1.positionAtReferencedTransform = true;
            Content.CreateAndAddEffectDef(slashEffectBasic);
            slashEffectBasic.transform.localScale *= 6;

            Transform slash = slashEffectBasic.transform.Find("SwingTrail");
            if (slash)
            {
                ParticleSystem ps = slash.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                ParticleSystemRenderer psr = slash.GetComponent<ParticleSystemRenderer>();

                Material ralseiSwipeMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercSwipe1.mat").WaitForCompletion());
                ralseiSwipeMaterial.SetColor("_TintColor", new Color32(40, 8, 17, 255));
                ralseiSwipeMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLaserTurbine.png").WaitForCompletion());

                psr.material = ralseiSwipeMaterial;
            }

            Transform sparks = slashEffectBasic.transform.Find("Sparks");
            if (sparks)
            {
                GameObject.Destroy(sparks.gameObject);
            }

            slashEffectCombo = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("RalseiScarfSpin");
            EffectComponent ec2 = slashEffectCombo.GetComponent<EffectComponent>();
            if(ec2 == null)
                ec2 = slashEffectCombo.AddComponent<EffectComponent>();
            ec2.parentToReferencedTransform = true;
            ec2.positionAtReferencedTransform = true;
            Content.CreateAndAddEffectDef(slashEffectCombo);

            Transform spin = slashEffectCombo.transform.Find("SwingTrail");
            if (spin)
            {
                ParticleSystem ps = spin.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.startSizeXMultiplier *= 1.5f;
                main.startSizeYMultiplier *= 1.5f;
                main.startSizeZMultiplier *= 1.5f;
                ParticleSystemRenderer psr = spin.GetComponent<ParticleSystemRenderer>();

                Material ralseiSpinMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercSwipe2.mat").WaitForCompletion());
                ralseiSpinMaterial.SetColor("_TintColor", new Color32(40, 22, 33, 190));
                ralseiSpinMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampDiamondLaser.png").WaitForCompletion());

                psr.material = ralseiSpinMaterial;
                //Material ralseiSpinMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/ColorRamps/texRampDiamondLaser.png").WaitForCompletion());
            }

            //slashImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("RalseiScarfImpact");
            //Content.CreateAndAddEffectDef(slashImpactEffect);
        }
    }
}
