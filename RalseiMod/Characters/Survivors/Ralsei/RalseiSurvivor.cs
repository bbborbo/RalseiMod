using BepInEx.Configuration;
using RalseiMod.Modules.Characters;
using RalseiMod.Modules;
using RalseiMod.Survivors.Ralsei.Components;
using RalseiMod.Survivors.Ralsei.SkillStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using RalseiMod.Skills;

namespace RalseiMod.Survivors.Ralsei
{
    public class RalseiSurvivor : SurvivorBase<RalseiSurvivor>
    {
        #region config
        public override string ConfigName => "Ralsei";

        [AutoConfig("Jump Power", "Ralsei's jump power. 15 is standard for most survivors.", 18f)]
        public static float ralseiJumpPower;
        [AutoConfig("Movement Speed", "Ralsei's movement speed. 11 is standard for most survivors.", 12f)]
        public static float ralseiMoveSpeed;
        #endregion
        #region language
        public override string CharacterName => "Ralsei";
        public override string SurvivorSubtitle => "Prince from The Dark";
        public override string SurvivorOutroWin => "..and so he left, shining with HOPE.";
        public override string SurvivorOutroFailure => "..and so he vanished, a whisper of legend fading to black.";
        public override string CharacterLore => "";
        public override string SurvivorDescription => "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease.";
        #endregion
        public override string bodyName => "RalseiBody"; 
        public override string masterName => "RalseiMonsterMaster";

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlHenry";
        public override string displayPrefabName => "HenryDisplay";

        public const string RALSEI_PREFIX = RalseiPlugin.DEVELOPER_PREFIX + "_RALSEI_";
        public override string survivorTokenPrefix => RALSEI_PREFIX;
        
        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyNameToClone = "Commando",
            bodyName = bodyName,
            bodyNameToken = RALSEI_PREFIX + "NAME",
            subtitleNameToken = RALSEI_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = Color.green,
            sortPosition = 100,

            crosshair = Assets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 75f,
            healthRegen = 1f,
            armor = 0f,
            moveSpeed = ralseiMoveSpeed,

            jumpCount = 1,
            jumpPower = ralseiJumpPower,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = assetBundle.LoadMaterial("matHenry"),
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                },
                new CustomRendererInfo
                {
                    childName = "Model",
                }
        };

        public override UnlockableDef characterUnlockableDef => RalseiUnlockables.characterUnlockableDef;
        
        public override ItemDisplaysBase itemDisplays => new HenryItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Init()
        {
            //loads the assetbundle then calls InitializeCharacter in CharacterBase
            base.Init();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            RalseiUnlockables.Init();

            ///
            /// Loads the asset bundle for this character, then
            ///     Initializes the body prefab and model
            ///     Initializes item displays
            ///     Initializes the display prefab (logbook? css?)
            /// And registers the survivor
            ///
            base.InitializeCharacter();

            HenryStates.Init();
            HenryTokens.Init();

            HenryAssets.Init(assetBundle);
            HenryBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            //bodyPrefab.AddComponent<HenryWeaponComponent>();
            //bodyPrefab.AddComponent<HuntressTrackerComopnent>();
            //anything else here
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            Modules.Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", "SwordHitbox");
        }

        public override void InitializeEntityStateMachines() 
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Modules.Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Modules.Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Modules.Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
        }
        public override void InitializeCharacterMaster() => HenryAI.Init(bodyPrefab, masterName);
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Modules.Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();
            if (skillLocator != null)
                Modules.Skills.characterSkillLocators[bodyName] = skillLocator;

            //add passive skill
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = RALSEI_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = RALSEI_PREFIX + "PASSIVE_DESCRIPTION",
                keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };
            //GenericSkill passiveGenericSkill = Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);
        }

        public override void Hooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }
        public override void Lang()
        {
            base.Lang();
        }
        
        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
                //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
                //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin
            
            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);
            
            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins


        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {

            if (sender.HasBuff(HenryBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }
        }
    }
}