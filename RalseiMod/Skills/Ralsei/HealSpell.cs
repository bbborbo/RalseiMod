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
    class HealSpell : SkillBase<HealSpell>
    {
        #region config
        public override string ConfigName => SkillName;

        //[AutoConfig("Step Count", 4)]
        public static int stepCount;
        #endregion
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Dual Heal";

        public override string SkillDescription => "Cast a healing spell on yourself and all allies within Xm, restoring Y% health and granting Regenerative for Z seconds.";

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
