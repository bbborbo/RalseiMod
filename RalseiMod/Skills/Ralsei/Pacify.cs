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

namespace RalseiMod.Skills
{
    class Pacify : SkillBase<Pacify>
    {
        #region config
        public override string ConfigName => "Skill : " + SkillName;

        [AutoConfig("Should Pacified Enemies Use Ambient Level", "If set to false, pacified enemies will be revived using player level. This can be used to balance enemy strength, if desired.", true)]
        public static bool useAmbientLevel;
        [AutoConfig("Sleep Conversion Delay", "The amount of seconds an enemy should sleep before converting to an ally.", 15)]
        public static float convertDelay;
        #endregion
        public static BuffDef spareBuff;
        public static BuffDef sleepyBuff;
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override string SkillName => "Pacify";

        public override string SkillDescription => 
            $"Has {UtilityColor("2")} charges. Target an enemy to put to {UtilityColor("Sleep")}. " +
            $"Spared enemies {DamageColor("convert to an Empowered ally")} after {UtilityColor(convertDelay.ToString())} seconds.";

        public override string SkillLangTokenName => "PACIFY";

        public override UnlockableDef UnlockDef => null;

        public override Sprite Icon => LoadSpriteFromRorSkill("RoR2/Base/Heretic/HereticDefaultAbility.asset");

        public override Type ActivationState => typeof(PacifySpell);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => RalseiSurvivor.instance.bodyName;

        public override SkillSlot SkillSlot => SkillSlot.Special;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            baseRechargeInterval = 40,
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

            spareBuff = ScriptableObject.CreateInstance<BuffDef>();
            spareBuff.name = "SpareBuff";
            spareBuff.isHidden = true;
            spareBuff.isDebuff = false;
            Content.AddBuffDef(spareBuff);

            sleepyBuff = ScriptableObject.CreateInstance<BuffDef>();
            spareBuff.name = "SleepyBuff";
            spareBuff.isDebuff = true;
            spareBuff.canStack = false;
            Content.AddBuffDef(spareBuff);
        }
        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += SpareHook;
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
