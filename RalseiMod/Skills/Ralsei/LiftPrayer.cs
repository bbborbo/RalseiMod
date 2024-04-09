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

namespace RalseiMod.Skills
{
    class LiftPrayer : SkillBase<LiftPrayer>
    {
        #region config
        public override string ConfigName => SkillName;

        //[AutoConfig("Step Count", 4)]
        public static int stepCount;
        #endregion
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Thread Whip";

        public override string SkillDescription => "";

        public override string SkillLangTokenName => "LIFTPRAYER";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(Idle);

        public override Type BaseSkillDef => typeof(ComboSkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Utility;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 0
        };

        public override void Init()
        {
            base.Init();
            ((ComboSkillDef)SkillDef).comboList = new ComboSkillDef.Combo[4]
            {
                Modules.Skills.ComboFromType(typeof(Idle)),
                Modules.Skills.ComboFromType(typeof(Idle)),
                Modules.Skills.ComboFromType(typeof(Idle)),
                Modules.Skills.ComboFromType(typeof(Idle)),
            };
        }
        public override void Hooks()
        {

        }
    }
}
