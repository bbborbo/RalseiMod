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
using static RalseiMod.Modules.Language.Styling;

namespace RalseiMod.Skills
{
    class ScarfRange : SkillBase<ScarfRange>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Damage : Base Attack Damage", 0.7f)]
        public static float baseDamage;
        [AutoConfig("Damage : Combo Attack Base Damage", 2f)]
        public static float baseDamageCombo;
        [AutoConfig("Damage : Base Attack Proc Coefficient", 1f)]
        public static float baseProcCoeff;
        [AutoConfig("Damage : Combo Attack Proc Coefficient", "Proc coefficient will affect the duration of the Painting status effect", 1f)]
        public static float comboProcCoeff;

        [AutoConfig("Combo : Combo Count", "The number of attacks in the Thread Whip attack combo. Combo attack will always be last. Min of 1.", 4)]
        public static int comboCount;
        [AutoConfig("Combo : Grace Duration", "The time in seconds that Thread Whip should wait after attacking for a new input that continues the combo. Min of 0.05", 0.15f)]
        public static float comboGraceDuration;

        [AutoConfig("Duration : Base Attack Entry Duration", 0f)]
        public static float baseEntryDuration;
        [AutoConfig("Duration : Base Attack Exit Duration", 0.45f)]
        public static float baseExitDuration;
        [AutoConfig("Duration : Combo Attack Entry Duration", 0.3f)]
        public static float comboEntryDuration;
        [AutoConfig("Duration : Combo Attack Exit Duration", 0.55f)]
        public static float comboExitDuration;
        #endregion
        internal static int lastCombo => comboCount - 1;
        public static GameObject tracerThread;
        public static GameObject tracerImpact;

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Thread Whip";

        public override string SkillDescription => 
            $"Use your scarf to throw {UtilityColor("magic threads")} for {DamageValueText(baseDamage)}. " +
            $"Every {DamageColor(NumToAdj(comboCount))} attack {UtilityColor("Paints")} enemies for {DamageValueText(baseDamageCombo)}.";

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
            base.Init();
            (SkillDef as SteppedSkillDef).stepCount = Mathf.Max(comboCount, 1);
            (SkillDef as SteppedSkillDef).stepGraceDuration = Mathf.Max(comboGraceDuration, 0.1f);

            CreateTracer();
        }

        private void CreateTracer()
        {
            tracerThread = Assets.CloneTracer("TracerGolem", "TracerRalseiThread");
            /*tracerThread = RoR2.LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerGolem").InstantiateClone("tracerRalseiThread", false);
            Tracer buckshotTracer = tracerThread.GetComponent<Tracer>();
            buckshotTracer.speed = 300f;
            buckshotTracer.length = 15f;
            buckshotTracer.beamDensity = 10f;
            VFXAttributes buckshotAttributes = tracerThread.AddComponent<VFXAttributes>();
            buckshotAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            buckshotAttributes.vfxIntensity = VFXAttributes.VFXIntensity.High;*/

            Particles.GetParticle(tracerThread, "SmokeBeam", new Color(0.81f, 0.4f, 1f), 0.66f);
            ParticleSystem.MainModule main = tracerThread.GetComponentInChildren<ParticleSystem>().main;
            main.startSizeXMultiplier *= 0.4f;
            main.startSizeYMultiplier *= 0.4f;
            main.startSizeZMultiplier *= 2f;
        }
        public override void Hooks()
        {

        }
    }
}
