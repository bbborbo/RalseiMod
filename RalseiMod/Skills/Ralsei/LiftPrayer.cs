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
using static R2API.RecalculateStatsAPI;

namespace RalseiMod.Skills
{
    class LiftPrayer : SkillBase<LiftPrayer>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Ability Cooldown", 6)]
        public static float cooldown;

        [AutoConfig("Ascent Duration", 1.5f)]
        public static float liftDuration;
        [AutoConfig("Ascent Rate Minimum", 0.2f)]
        public static float liftSpeedMin;
        [AutoConfig("Ascent Rate Maximum", 4f)]
        public static float liftSpeedMax;
        [AutoConfig("Ascent Rate Affected By Movement Speed", "Should Ralsei's Ascent State be affected by movement speed?", false)]
        public static bool useMoveSpeed;
        [AutoConfig("Hover Vertical Speed", -6f)]
        public static float hoverVelocity;
        [AutoConfig("Hover Acceleration", "How fast should Ralsei accelerate towards the hover vertical speed while hovering", 30)]
        public static float hoverAcceleration;
        [AutoConfig("Hover Horizontal Speed Increase", 1.3f)]
        public static float hoverSpeedBoost;
        #endregion
        public static BuffDef hoverBuff;
        public static SkillDef cancelSkillDef => CancelHoverSkill.instance.SkillDef;
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Hover Prayer";

        public override string SkillDescription => 
            $"Ascend {UtilityColor("high into the air")}, " +
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
            baseRechargeInterval = cooldown,
            beginSkillCooldownOnSkillEnd = true,
            fullRestockOnAssign = false,
            mustKeyPress = true,
            interruptPriority = InterruptPriority.Any,
            cancelSprintingOnActivation = false,
            isCombatSkill = false
        };

        public override void Init()
        {
            base.Init();

            hoverBuff = Content.CreateAndAddBuff("RalseiHoverSpeed", null, Color.green, true, false);
            hoverBuff.isHidden = true;
            Content.AddEntityState(typeof(States.Ralsei.HoverState));
        }
        public override void Hooks()
        {
            GetStatCoefficients += HoverSpeedBoost;
        }

        private void HoverSpeedBoost(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(hoverBuff))
                args.moveSpeedMultAdd += hoverSpeedBoost - 1;
        }
    }
}
