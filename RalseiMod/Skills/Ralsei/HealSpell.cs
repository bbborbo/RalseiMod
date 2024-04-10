using BepInEx.Configuration;
using EntityStates;
using RalseiMod.Modules;
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
    class HealSpell : SkillBase<HealSpell>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Heal Range", 100f)]
        public static float healRange;

        [AutoConfig("Immediate Heal Fraction", 0.2f)]
        public static float instantHealPercent;

        [AutoConfig("Passive Heal Duration", 3f)]
        public static float healDuration;
        #endregion
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Dual Heal";

        public override string SkillDescription => 
            $"Cast a {HealingColor("healing spell")} on yourself and all allies within {UtilityColor(healRange + "m")}, " +
            $"restoring {HealingColor(ConvertDecimal(instantHealPercent) + " health")} " +
            $"and granting {HealingColor("Regenerative")} for {HealingColor(healDuration.ToString())} seconds.";

        public override string SkillLangTokenName => "HEALSPELL";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(Idle);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Secondary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 0
        };

        public override void Init()
        {
            base.Init();
        }
        public override void Hooks()
        {

        }
    }
}
