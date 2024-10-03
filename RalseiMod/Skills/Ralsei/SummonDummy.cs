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

namespace RalseiMod.Skills.Ralsei
{
    class SummonDummy : SkillBase<SummonDummy>
    {
        public DeployableAPI.GetDeployableSameSlotLimit GetDummySlotLimit;
        public static DeployableSlot dummyDeployableSlot;

        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Ability Stock", 2)]
        public static int stock;

        [AutoConfig("Ability Cooldown", 30)]
        public static float cooldown;

        [AutoConfig("Prepare Duration", 1)]
        public static float prepareDuration;

        [AutoConfig("Throw Force", 60f)]
        public static float throwForce;

        [AutoConfig("Throw Duration", 0.5f)]
        public static float throwDuration;

        [AutoConfig("Minion Max Base", "The maximum amount of dummy minions Ralsei should be allowed to have at base.", 2)]
        public static int maxMinionBase;
        [AutoConfig("Minion Max Upgraded", "The maximum amount of dummy minions Ralsei should be allowed to have with at least one lysate cell.", 3)]
        public static int maxMinionUpgrade;

        #endregion
        public override string SkillName => "Practice Dummy";

        public override string SkillDescription => $"Throw a practice dummy that draws ALL enemy attacks, " +
            $"creating a Stunning and Fatiguing blast when it dies. " +
            $"Periodically Empowers nearby allies.";

        public override string SkillLangTokenName => "RALSEIDUMMYSKILL";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Engi/EngiBodyPlaceTurret.asset");

        public override Type ActivationState => typeof(ThrowDummy);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Special;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            baseRechargeInterval = cooldown,
            fullRestockOnAssign = false,
            beginSkillCooldownOnSkillEnd = true,
            isCombatSkill = false,
            canceledFromSprinting = false,
            cancelSprintingOnActivation = true,
            mustKeyPress = true,
            interruptPriority = InterruptPriority.Skill,
            baseMaxStock = stock
        };

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override void Init()
        {
            KeywordTokens = new string[] { "KEYWORD_STUNNING", RalseiSurvivor.fatigueKeywordToken, RalseiSurvivor.empowerKeywordToken };
            base.Init();

            GetDummySlotLimit += GetMaxDummyMinions;
            dummyDeployableSlot = DeployableAPI.RegisterDeployableSlot(GetDummySlotLimit);
        }

        private int GetMaxDummyMinions(CharacterMaster self, int deployableCountMultiplier)
        {
            return self.inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid) > 0 ? maxMinionUpgrade : maxMinionBase;
        }

        public override void Hooks()
        {

        }
    }
}
