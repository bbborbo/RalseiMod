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
    class Pacify : SkillBase<Pacify>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Sleep Conversion Delay", "The amount of seconds an enemy should sleep before converting to an ally.", 15)]
        public static float convertDelay;
        #endregion
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Pacify";

        public override string SkillDescription => 
            $"Has {UtilityColor("2")} charges. Target an enemy to put to {UtilityColor("Sleep")}. " +
            $"Spared enemies {DamageColor("convert to an Empowered ally")} after {UtilityColor(convertDelay.ToString())} seconds.";

        public override string SkillLangTokenName => "PACIFY";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(Idle);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Special;

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
