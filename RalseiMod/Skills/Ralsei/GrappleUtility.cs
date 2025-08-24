using EntityStates;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RalseiMod.Modules.Language.Styling;
using RalseiMod.States.Ralsei.Weapon;

namespace RalseiMod.Skills.Ralsei
{
    class GrappleUtility : SkillBase<GrappleUtility>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;
        #endregion
        public override string SkillName => "Scarf Sling";

        public override string SkillDescription => $"Target an ally to grapple to, dealing {DamageValueText(0)} to enemies in your path. " +
            $"Then, {DamageColor("stun")} nearby enemies for {DamageValueText(0)}, and hover to the ground.";

        public override string SkillLangTokenName => "SCARFSLING";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Loader/FireHook.asset");

        public override Type ActivationState => typeof(GrappleChuck);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.CharacterName;

        public override SkillSlot SkillSlot => SkillSlot.Utility;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            /*stockToConsume = 1,
            baseRechargeInterval = 2,
            resetCooldownTimerOnUse = false,
            canceledFromSprinting = true,
            cancelSprintingOnActivation = true,
            baseMaxStock = 2,
            isCombatSkill = false,
            interruptPriority = InterruptPriority.PrioritySkill*/
        };

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override void Hooks()
        {

        }
    }
}
