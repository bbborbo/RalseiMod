using BepInEx.Configuration;
using EntityStates;
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
using static R2API.RecalculateStatsAPI;
using R2API;

namespace RalseiMod.Skills
{
    class Pacify : SkillBase<Pacify>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Ability Cooldown", 30)]
        public static float cooldown;

        [AutoConfig("Should Pacified Enemies Use Ambient Level", "If set to false, pacified enemies will be revived using player level. This can be used to balance enemy strength, if desired.", true)]
        public static bool useAmbientLevel;
        [AutoConfig("Sleep Conversion Delay", "The amount of seconds an enemy should sleep before converting to an ally.", 5)]
        public static float convertDelay;

        [AutoConfig("Minion Max Base", "The maximum amount of pacified minions Ralsei should be allowed to have at base.", 3)]
        public static int maxMinionBase;
        [AutoConfig("Minion Max Upgraded", "The maximum amount of pacified minions Ralsei should be allowed to have with at least one lysate cell.", 5)]
        public static int maxMinionUpgrade;
        [AutoConfig("Duplicate Pacified Minions With Swarms", true)]
        public static bool swarmsDuplicate;
        #endregion
        public static BuffDef spareBuff;
        public static DeployableSlot pacifyDeployableSlot;

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Pacify";

        public override string SkillDescription => 
            $"Has {UtilityColor("2")} charges. Cast a {UtilityColor("Sleeping")} spell on an enemy below {UtilityColor("50% health")}. " +
            $"Pacified enemies {DamageColor("become Empowered allies")} after {UtilityColor(convertDelay.ToString())}s. " +
            $"Max of {maxMinionBase}.";

        public override string SkillLangTokenName => "PACIFY";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Heretic/HereticDefaultAbility.asset");

        public override Type ActivationState => typeof(PreparePacifySpell);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Special;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            baseRechargeInterval = cooldown,
            resetCooldownTimerOnUse = false,
            canceledFromSprinting = true,
            cancelSprintingOnActivation = true,
            baseMaxStock = 2,
            isCombatSkill = false,
            interruptPriority = InterruptPriority.PrioritySkill
        };

        public override void Init()
        {
            base.Init();
            Content.AddEntityState(typeof(CastPacifySpell));

            spareBuff = ScriptableObject.CreateInstance<BuffDef>();
            spareBuff.name = "SpareBuff";
            spareBuff.isHidden = true;
            spareBuff.isDebuff = false;
            Content.AddBuffDef(spareBuff);

            GetPacifySlotLimit += GetMaxPacifyMinions;
            pacifyDeployableSlot = DeployableAPI.RegisterDeployableSlot(GetPacifySlotLimit);
        }

        private int GetMaxPacifyMinions(CharacterMaster self, int deployableCountMultiplier)
        {
            int i = maxMinionBase;
            if (self.inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid) > 0)
                i = maxMinionUpgrade;

            if (swarmsDuplicate && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Swarms))
            {
                i *= 2;
            }
            return i;
        }

        public DeployableAPI.GetDeployableSameSlotLimit GetPacifySlotLimit;
        public override void Hooks()
        {
            GetStatCoefficients += StatsHook;
            On.RoR2.GlobalEventManager.OnCharacterDeath += SpareHook;
        }

        private void StatsHook(CharacterBody sender, StatHookEventArgs args)
        {
        }

        private void SpareHook(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if (damageReport.victimBody.HasBuff(spareBuff))
            {
                return;
            }
            orig(self, damageReport);
        }
    }
}
