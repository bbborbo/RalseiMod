using BepInEx.Configuration;
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
    class ScarfRange : SkillBase<ScarfRange>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Damage : Base Attack Damage", 1.3f)]
        public static float baseDamage;
        [AutoConfig("Damage : Finisher Attack Base Damage", 1.8f)]
        public static float baseDamageFinisher;
        [AutoConfig("Damage : Base Attack Proc Coefficient", 0.8f)]
        public static float procCoeffBase;
        [AutoConfig("Damage : Finisher Attack Proc Coefficient", "Proc coefficient will affect the duration of the Tangled status effect", 1f)]
        public static float procCoeffFinisher;

        public static int comboCount = 4;
        [AutoConfig("Combo : Grace Duration", "The time in seconds that Thread Whip should wait after attacking for a new input that continues the combo. Min of 0.02", 0.1f)]
        public static float comboGraceDuration;

        [AutoConfig("Duration : Base Attack Entry Duration", 0.1f)]
        public static float baseEntryDuration;
        [AutoConfig("Duration : Base Attack Exit Duration", 0.26f)]
        public static float baseExitDuration;
        [AutoConfig("Duration : Finisher Attack Entry Duration", 0.4f)]
        public static float comboEntryDuration;
        [AutoConfig("Duration : Finisher Attack Exit Duration", 0.7f)]
        public static float comboExitDuration;
        #endregion
        internal static int lastCombo => comboCount - 1;
        public static GameObject tracerThread;
        public static GameObject tracerThreadFinisher;
        public static GameObject muzzleFlash;
        public static GameObject muzzleFlashFinisher;
        public static GameObject threadImpact;
        public static GameObject threadImpactFinisher;
        public static GameObject swingEffect;
        public static GameObject swingEffectFinisher;

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Thread Whip";

        public override string SkillDescription => 
            $"Use your scarf to sling {UtilityColor("piercing threads")} for {DamageValueText(baseDamage)}. " +
            $"Every {DamageColor(NumToAdj(comboCount))} attack {UtilityColor("Tangles")} enemies for {DamageValueText(baseDamageFinisher)}.";

        public override string SkillLangTokenName => "SCARFRANGE";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeHeavy.asset");

        public override Type ActivationState => typeof(ScarfAttackLong);

        public override Type BaseSkillDef => typeof(SteppedSkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Primary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 0,
            interruptPriority = InterruptPriority.Any
        };

        public override void Init()
        {
            KeywordTokens = new string[] { RalseiSurvivor.tangleKeywordToken };
            base.Init();
            (SkillDef as SteppedSkillDef).stepCount = 4;// Mathf.Max(comboCount, 1);
            (SkillDef as SteppedSkillDef).stepGraceDuration = Mathf.Max(comboGraceDuration, 0.02f);

            CreateTracers();
            CreateSwingEffects();
            CreateMuzzleFlashes();
            CreateImpactEffect();
        }

        private void CreateImpactEffect()
        {
            threadImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FlyingVermin/VerminSpitImpactEffect.prefab").WaitForCompletion().InstantiateClone("RalseiScarfThreadImpact");
            EffectComponent threadImpactEffect = threadImpact.GetComponent<EffectComponent>();
            threadImpactEffect.soundName = "";

            Content.CreateAndAddEffectDef(threadImpact);

            Transform goo = threadImpact.transform.Find("Goo");
            if (goo)
            {
                GameObject.Destroy(goo.gameObject);
            }
            Transform light = threadImpact.transform.Find("Point Light");
            if (light)
            {
                Light l = light.GetComponent<Light>();
                l.color = new Color32(255, 74, 249, 255);
            }
            Transform flash = threadImpact.transform.Find("Flash");
            if (flash)
            {
                ParticleSystemRenderer psr = flash.GetComponent<ParticleSystemRenderer>();
                if (psr)
                {
                    Material mat = UnityEngine.Object.Instantiate(psr.material);
                    psr.material = mat;
                    mat.DisableKeyword("VERTEXCOLOR");
                    mat.SetFloat("_VertexColorOn", 0);
                    mat.SetColor("_TintColor", new Color32(255, 74, 249, 255));
                }
            }
            Transform ringMesh = threadImpact.transform.Find("Ring, Mesh");
            if (ringMesh)
            {
                ParticleSystem ps = ringMesh.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.startSizeMultiplier *= 0.6f;

                ParticleSystemRenderer psr = ringMesh.GetComponent<ParticleSystemRenderer>();
                if (psr)
                {
                    Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Junk/AncientWisp/matAncientWillowispSpiral.mat").WaitForCompletion());
                    psr.material = mat;

                    mat.name = "RalseiThreadImpactSpiral";
                    mat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/DLC2/Scorchling/texRampScorchling.png").WaitForCompletion());
                    mat.SetColor("_TintColor", new Color32(127, 0, 166, 201));
                }
                GameObject ringMesh2 = GameObject.Instantiate(ringMesh.gameObject, threadImpact.transform);
            }
        }

        private void CreateMuzzleFlashes()
        {
            muzzleFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion().InstantiateClone("RalseiScarfMuzzleFlash");
            Content.CreateAndAddEffectDef(muzzleFlash);
            muzzleFlash.transform.localScale *= 1.3f;

            Particles.GetParticle(muzzleFlash, "Hitflash", new Color32(150, 83, 122, 255), 1.5f);
            Particles.GetParticle(muzzleFlash, "Starburst", new Color32(150, 83, 122, 255), 1.5f);

            muzzleFlashFinisher = muzzleFlash.InstantiateClone("RalseiScarfMuzzleFlashFinisher");
            muzzleFlashFinisher.transform.localScale *= 1.5f;

            Particles.GetParticle(muzzleFlashFinisher, "Hitflash", new Color32(122, 83, 150, 255), 2);
            Particles.GetParticle(muzzleFlashFinisher, "Starburst", new Color32(122, 83, 150, 255), 2);
            Content.CreateAndAddEffectDef(muzzleFlashFinisher);
            return;
            muzzleFlashFinisher = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainChargeTazer.prefab").WaitForCompletion().InstantiateClone("RalseiScarfMuzzleFlashFinisher");

            Transform charge = muzzleFlashFinisher.transform.Find("Charge");
            if (charge)
            {
                GameObject.Destroy(charge.gameObject);
            }
            Particles.GetParticle(muzzleFlashFinisher, "Rings (1)", new Color32(200, 110, 165, 255), 1);
            Particles.GetParticle(muzzleFlashFinisher, "Core (1)", new Color32(200, 110, 165, 255), 1);

            Content.CreateAndAddEffectDef(muzzleFlashFinisher);
        }

        private void CreateSwingEffects()
        {
            swingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSwing1, Kickup.prefab").WaitForCompletion().InstantiateClone("RalseiScarfSling");
            EffectComponent ec1 = swingEffect.AddComponent<EffectComponent>();
            ec1.parentToReferencedTransform = true;
            ec1.positionAtReferencedTransform = true;
            Content.CreateAndAddEffectDef(swingEffect);

            Transform distortion = swingEffect.transform.Find("SwingTrail, Distortion");
            if (distortion)
            {
                distortion.SetPositionAndRotation(new Vector3(5,5,0), distortion.rotation);

                ParticleSystem ps = distortion.GetComponent<ParticleSystem>();
                ParticleSystemRenderer psr = distortion.GetComponent<ParticleSystemRenderer>();

                Material ralseiSwipeMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonbatteryGlassDistortion.mat").WaitForCompletion();

                psr.material = ralseiSwipeMaterial;
            }
            Transform slash = swingEffect.transform.Find("SwingTrail");
            if (slash)
            {
                GameObject.DestroyImmediate(slash.gameObject);
            }
            Transform debris = swingEffect.transform.Find("Debris");
            if (debris)
            {
                GameObject.DestroyImmediate(debris.gameObject);
            }
            Transform dust = swingEffect.transform.Find("Dust");
            if (dust)
            {
                GameObject.DestroyImmediate(dust.gameObject);
            }
            Transform physics = swingEffect.transform.Find("Physics");
            if (physics)
            {
                GameObject.DestroyImmediate(physics.gameObject);
            }
            Transform pp = swingEffect.transform.Find("PP");
            if (pp)
            {
                GameObject.DestroyImmediate(pp.gameObject);
            }

            swingEffectFinisher = swingEffect.InstantiateClone("RalseiScarfSlingFinisher");
            Content.CreateAndAddEffectDef(swingEffectFinisher);

            swingEffect.transform.localScale *= 0.1f;
            swingEffectFinisher.transform.localScale *= 0.15f;
        }

        private void CreateTracers()
        {
            tracerThread = Modules.Assets.CloneTracer("TracerToolbotRebar", "TracerRalseiThread");

            Tracer t1 = tracerThread.GetComponent<Tracer>();
            if (t1)
            {
                //t1.speed = 250f;
                //t1.length = 50f;
            }
            /*tracerThread = RoR2.LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerGolem").InstantiateClone("tracerRalseiThread", false);
            Tracer buckshotTracer = tracerThread.GetComponent<Tracer>();
            buckshotTracer.speed = 300f;
            buckshotTracer.length = 15f;
            buckshotTracer.beamDensity = 10f;
            VFXAttributes buckshotAttributes = tracerThread.AddComponent<VFXAttributes>();
            buckshotAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            buckshotAttributes.vfxIntensity = VFXAttributes.VFXIntensity.High;*/
            Transform stickEffect1 = tracerThread.transform.Find("StickEffect");
            if (stickEffect1)
            {
                GameObject.Destroy(stickEffect1.gameObject);
            }
            Transform tracerhead1 = tracerThread.transform.Find("TracerHead");
            if (tracerhead1)
            {
            }
            Transform beamObject1 = tracerThread.transform.Find("BeamObject");
            if (beamObject1)
            {
                ParticleSystem ps = beamObject1.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.startLifetime = 0.4f;

                ParticleSystemRenderer psr = beamObject1.GetComponent<ParticleSystemRenderer>();

                Material ralseiSwipeMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Toolbot/matRebarTrail.mat").WaitForCompletion());
                ralseiSwipeMaterial.SetColor("_TintColor", new Color32(81, 16, 35, 255));
                ralseiSwipeMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLaserTurbine.png").WaitForCompletion());

                psr.trailMaterial = ralseiSwipeMaterial;
            }

            tracerThreadFinisher = Modules.Assets.CloneTracer("TracerToolbotRebar", "TracerRalseiThreadFinisher");

            Tracer t2 = tracerThreadFinisher.GetComponent<Tracer>();
            if (t2)
            {
                //t2.speed = 600f;
                //t2.length = 10f;
            }
            /*tracerThread = RoR2.LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerGolem").InstantiateClone("tracerRalseiThread", false);
            Tracer buckshotTracer = tracerThread.GetComponent<Tracer>();
            buckshotTracer.speed = 300f;
            buckshotTracer.length = 15f;
            buckshotTracer.beamDensity = 10f;
            VFXAttributes buckshotAttributes = tracerThread.AddComponent<VFXAttributes>();
            buckshotAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            buckshotAttributes.vfxIntensity = VFXAttributes.VFXIntensity.High;*/

            Transform stickEffect = tracerThreadFinisher.transform.Find("StickEffect");
            if (stickEffect)
            {
                GameObject.Destroy(stickEffect.gameObject);
            }
            Transform tracerhead2 = tracerThreadFinisher.transform.Find("TracerHead");
            if (tracerhead2)
            {
            }
            Transform beamObject2 = tracerThreadFinisher.transform.Find("BeamObject");
            if (beamObject2)
            {
                ParticleSystem ps = beamObject2.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.startLifetime = 1f;

                ParticleSystemRenderer psr = beamObject2.GetComponent<ParticleSystemRenderer>();

                Material ralseiSwipeMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Toolbot/matRebarTrail.mat").WaitForCompletion());
                ralseiSwipeMaterial.SetColor("_TintColor", new Color32(43, 42, 105, 255));
                ralseiSwipeMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampDiamondLaser.png").WaitForCompletion());

                psr.trailMaterial = ralseiSwipeMaterial;
            }
        }
        public override void Hooks()
        {

        }
    }
}
