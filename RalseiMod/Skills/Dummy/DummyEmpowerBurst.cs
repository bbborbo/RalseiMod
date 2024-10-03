using EntityStates;
using RalseiMod.Characters;
using RalseiMod.Modules;
using RalseiMod.States.Dummy;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.Skills.Dummy
{
    class DummyEmpowerBurst : SkillBase<DummyEmpowerBurst>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Ability Cooldown", 12)]
        public static float cooldown;

        [AutoConfig("Empower Duration", 6)]
        public static float empowerDuration;

        [AutoConfig("Empower Range", 80f)]
        public static float empowerRange;
        #endregion

        public override string SkillName => "Dummy Empower Burst";

        public override string SkillDescription => "Empower nearby allies.";

        public override string SkillLangTokenName => "DUMMYEMPOWER";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Loader/GroundSlam.asset");

        public override Type ActivationState => typeof(DummyEmpowerBurstState);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => Characters.Dummy.instance.bodyName;//RalseiMod.Survivors.Dummy.DummySurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Primary;
        public override string ActivationStateMachineName => "Body";

        public override SimpleSkillData SkillData => new SimpleSkillData
        {
            requiredStock = 1,
            rechargeStock = 1,
            baseMaxStock = 1,
            dontAllowPastMaxStocks = true,
            fullRestockOnAssign = true,
            baseRechargeInterval = cooldown,
            stockToConsume = 1,
            interruptPriority = InterruptPriority.Pain
        };

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override void Init()
        {
            base.Init();
        }
        public override void Hooks()
        {
        }
    }
}
