using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RalseiMod.Modules;

namespace RalseiMod.Skills
{
    public abstract class SkillBase<T> : SharedBase<T> where T : SkillBase<T>, new()
{
        public static string Token = RalseiPlugin.DEVELOPER_PREFIX + "SKILL";
        public abstract string SkillName { get; }
        public abstract string SkillDescription { get; }
        public abstract string SkillLangTokenName { get; }

        //public abstract string UnlockString { get; }
        public abstract UnlockableDef UnlockDef { get; }
        public abstract string IconName { get; }
        public abstract Type ActivationState { get; }
        public abstract Type BaseSkillDef { get; }
        public abstract string CharacterName { get; }
        public abstract SkillSlot SkillSlot { get; }
        public abstract SimpleSkillData SkillData { get; }
        public string[] KeywordTokens;
        public virtual bool useSteppedDef { get; set; } = false;
        public SkillDef SkillDef;

        public override void Lang()
        {
            LanguageAPI.Add(Token + SkillLangTokenName, SkillName);
            LanguageAPI.Add(Token + SkillLangTokenName + "_DESCRIPTION", SkillDescription);
        }

        protected void CreateSkill()
        {
            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            if (useSteppedDef)
            {
                SkillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();
            }

            Content.AddEntityState(ActivationState);
            SkillDef.activationState = new SerializableEntityStateType(ActivationState);

            SkillDef.skillNameToken = Token + SkillLangTokenName;
            SkillDef.skillName = SkillName;
            SkillDef.skillDescriptionToken = Token + SkillLangTokenName + "_DESCRIPTION";
            SkillDef.activationStateMachineName = "Weapon";

            SkillDef.keywordTokens = KeywordTokens;
            SkillDef.icon = assetBundle.LoadAsset<Sprite>(RalseiPlugin.iconsPath + "Skill/" + IconName + ".png");

            #region SkillData
            SkillDef.baseMaxStock = SkillData.baseMaxStock;
            SkillDef.baseRechargeInterval = SkillData.baseRechargeInterval;
            SkillDef.beginSkillCooldownOnSkillEnd = SkillData.beginSkillCooldownOnSkillEnd;
            SkillDef.canceledFromSprinting = RalseiPlugin.autoSprintLoaded ? false : SkillData.canceledFromSprinting;
            SkillDef.cancelSprintingOnActivation = SkillData.cancelSprintingOnActivation;
            SkillDef.dontAllowPastMaxStocks = SkillData.dontAllowPastMaxStocks;
            SkillDef.fullRestockOnAssign = SkillData.fullRestockOnAssign;
            SkillDef.interruptPriority = SkillData.interruptPriority;
            SkillDef.isCombatSkill = SkillData.isCombatSkill;
            SkillDef.mustKeyPress = SkillData.mustKeyPress;
            SkillDef.rechargeStock = SkillData.rechargeStock;
            SkillDef.requiredStock = SkillData.requiredStock;
            SkillDef.resetCooldownTimerOnUse = SkillData.resetCooldownTimerOnUse;
            SkillDef.stockToConsume = SkillData.stockToConsume;
            #endregion

            Content.AddSkillDef(SkillDef);
            AddSkillDefToCharacter();
        }

        private void AddSkillDefToCharacter()
        {
            if (CharacterName == "")
                return;

            SkillLocator skillLocator;
            if (Modules.Skills.characterSkillLocators.ContainsKey(CharacterName))
            {
                skillLocator = Modules.Skills.characterSkillLocators[CharacterName];
            }
            else
            {
                GameObject body = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/" + CharacterName);
                skillLocator = body?.GetComponent<SkillLocator>();

                if (skillLocator)
                {
                    Modules.Skills.characterSkillLocators.Add(CharacterName, skillLocator);
                }
            }

            if (skillLocator != null)
            {
                SkillFamily skillFamily = null;

                switch (SkillSlot)
                {
                    case SkillSlot.Primary:
                        skillFamily = skillLocator.primary.skillFamily;
                        break;
                    case SkillSlot.Secondary:
                        skillFamily = skillLocator.secondary.skillFamily;
                        break;
                    case SkillSlot.Utility:
                        skillFamily = skillLocator.utility.skillFamily;
                        break;
                    case SkillSlot.Special:
                        skillFamily = skillLocator.special.skillFamily;
                        break;
                    case SkillSlot.None:
                        Debug.Log("Special case!");
                        break;
                }

                if (skillFamily != null)
                {
                    //Debug.Log(s);

                    Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
                    skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
                    {
                        skillDef = SkillDef,
                        unlockableDef = UnlockDef,
                        viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
                    };
                }
                else
                {
                    Debug.Log($"No skill family {SkillSlot.ToString()} found from " + CharacterName);
                }
            }
            else
            {
                Debug.Log("No skill locator found from " + CharacterName);
            }
        }

        internal UnlockableDef GetUnlockDef(Type type)
        {
            UnlockableDef u = null;

            /*foreach (KeyValuePair<UnlockBase, UnlockableDef> keyValuePair in Main.UnlockBaseDictionary)
            {
                string key = keyValuePair.Key.ToString();
                UnlockableDef value = keyValuePair.Value;
                if (key == type.ToString())
                {
                    u = value;
                    //Debug.Log($"Found an Unlock ID Match {value} for {type.Name}! ");
                    break;
                }
            }*/

            return u;
        }
    }
}
