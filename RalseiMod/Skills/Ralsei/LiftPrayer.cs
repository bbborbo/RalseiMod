using BepInEx.Configuration;
using EntityStates;
using RalseiMod.Modules;
using RalseiMod.States.Ralsei;
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
    class LiftPrayer : SkillBase<LiftPrayer>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Lift Duration", 2f)]
        public static float liftDuration;
        [AutoConfig("Lift Speed", 3.5f)]
        public static float liftSpeed;
        [AutoConfig("Hover Velocity", -5.5f)]
        public static float hoverVelocity;
        [AutoConfig("Hover Acceleration", 30)]
        public static float hoverAcceleration;
        #endregion
        public static SkillDef cancelSkillDef => CancelHoverSkill.instance.SkillDef;
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Hover Prayer";

        public override string SkillDescription => 
            $"Float {UtilityColor("high into the air")} for a short time, " +
            $"then slowly {UtilityColor("hover")} back down. " +
            $"{DamageColor("Hover effect ends upon landing or recasting the ability")}.";

        public override string SkillLangTokenName => "LIFTPRAYER";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Mage/MageBodyFlyUp.asset");

        public override Type ActivationState => typeof(LiftState);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Utility;
        public override string ActivationStateMachineName => "Body";

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            baseRechargeInterval = 9,
            beginSkillCooldownOnSkillEnd = true,
            fullRestockOnAssign = false,
            mustKeyPress = true,
            interruptPriority = InterruptPriority.Any
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
