using BepInEx.Configuration;
using RalseiMod.Modules;
using RalseiMod.Survivors.Ralsei.Components;
using RalseiMod.Survivors.Ralsei.SkillStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using RalseiMod.Skills;
using RalseiMod.Achievements;
using static RalseiMod.Modules.Language.Styling;
using RalseiMod.Characters;
using UnityEngine.AddressableAssets;
using R2API;

namespace RalseiMod.Survivors.Ralsei
{
    public class RalseiSurvivor : SurvivorBase<RalseiSurvivor>
    {
        #region config
        public override string ConfigName => "Survivor : " + CharacterName;

        [AutoConfig("Jump Power", "Ralsei's jump power. 15 is standard for most survivors.", 19f)]
        public static float ralseiJumpPower;
        [AutoConfig("Movement Speed", "Ralsei's movement speed. 7 is standard for most survivors.", 8f)]
        public static float ralseiMoveSpeed;
        [AutoConfig("Base Health", "Ralsei's base health. 110 is standard for most survivors.", 55f)]
        public static float ralseiBaseHealth;
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
        public static BuffDef empowerBuff;
        public static Material empowerEffectMaterial;
        public override string bodyName => "RalseiBody"; 
        public override string masterName => "RalseiMonsterMaster";

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlRalsei";
        public override string displayPrefabName => "RalseiDisplay";

        public const string RALSEI_PREFIX = RalseiPlugin.DEVELOPER_PREFIX + "_RALSEI_";
        public override string survivorTokenPrefix => RALSEI_PREFIX;
        
        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyNameToClone = "Commando",
            bodyName = bodyName,
            bodyNameToken = RALSEI_PREFIX + "NAME",
            subtitleNameToken = RALSEI_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = new Color32(0,255,127,100),
            sortPosition = 100,

            crosshair = Assets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = ralseiBaseHealth,
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
                //uses the name from ChildLocator
                childName = "Model"
            }
        };

        public override UnlockableDef characterUnlockableDef => RalseiUnlockables.characterUnlockableDef;
        
        public override ItemDisplaysBase itemDisplays => new RalseiItemDisplays();

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

            empowerBuff = Content.CreateAndAddBuff("RalseiEmpower", null, Color.yellow, true, false);

            empowerEffectMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/CritOnUse/matFullCrit.mat").WaitForCompletion());

            empowerEffectMaterial.SetColor("_TintColor", new Color32(150, 110, 0, 191));
            empowerEffectMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampBanditSplatter.png").WaitForCompletion());
            LanguageAPI.Add(RALSEI_PREFIX + "EMPOWERED_MODIFIER", "Empowered {0}");


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
            //Modules.Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", "SwordHitbox");
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
            {
                Modules.Skills.characterSkillLocators[bodyName] = skillLocator;

                #region Achievements
                Modules.Language.Add(RALSEI_PREFIX + "PASSIVE_NAME", $"Tension Points: Arcana");
                Modules.Language.Add(RALSEI_PREFIX + "PASSIVE_DESCRIPTION", 
                    $"{UtilityColor("Blocking")} attacks or {UtilityColor("Threading")} enemies will {DamageColor("reduce your skill cooldowns")}.");
                #endregion

                //add passive skill
                skillLocator.passiveSkill = new SkillLocator.PassiveSkill
                {
                    enabled = false,
                    skillNameToken = RALSEI_PREFIX + "PASSIVE_NAME",
                    skillDescriptionToken = RALSEI_PREFIX + "PASSIVE_DESCRIPTION",
                    icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
                };
            }
            //GenericSkill passiveGenericSkill = Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);
        }

        public override void Hooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterModel.UpdateOverlays += EmpowerOverlay;
            //On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += EmpowerVisualEffect;
            On.RoR2.Util.GetBestBodyName += EmpowermentNameModifier;
        }

        private string EmpowermentNameModifier(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            string name = orig(bodyObject);
            CharacterBody body = bodyObject?.GetComponent<CharacterBody>();
            if(body && body.HasBuff(empowerBuff))
            {
                name = RoR2.Language.GetStringFormatted(RALSEI_PREFIX + "EMPOWERED_MODIFIER", name);
            }
            return name;
        }

        private void EmpowerOverlay(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);
            if (self.visibility == VisibilityLevel.Invisible || self.body == null)
            {
                return;
            }

            AddOverlay(empowerEffectMaterial, self.body.HasBuff(empowerBuff));

            void AddOverlay(Material overlayMaterial, bool condition)
            {
                if (self.activeOverlayCount < CharacterModel.maxOverlays && condition)
                {
                    self.currentOverlays[self.activeOverlayCount++] = overlayMaterial;
                }
            }
        }

        private void EmpowerVisualEffect(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);
            return;
            self.UpdateSingleTemporaryVisualEffect(
                ref self.warbannerEffectInstance, 
                CharacterBody.AssetReferences.teamWarCryEffectPrefab,
                self.radius, self.HasBuff(empowerBuff), "");
        }

        public override void Lang()
        {
            base.Lang();
        }
        
        #region skins
        public override void InitializeSkins()
        {
            #region Tokens
            Modules.Language.Add(GetAchievementNameToken(RalseiMasteryAchievement.identifier), $"{CharacterName}: Mastery");
            Modules.Language.Add(GetAchievementDescriptionToken(RalseiMasteryAchievement.identifier), $"As {CharacterName}, beat the game or obliterate on Monsoon.");

            Modules.Language.Add(RALSEI_PREFIX + "DEFAULT_SKIN_NAME", $"Default");
            Modules.Language.Add(RALSEI_PREFIX + "MASTERY_SKIN_NAME", $"Peeled");
            Modules.Language.Add(RALSEI_PREFIX + "NIKO_SKIN_NAME", $"Solstice");
            #endregion

            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(RALSEI_PREFIX + "DEFAULT_SKIN_NAME",
                R2API.Skins.CreateSkinIcon(Color.black, Color.green, Color.green, Color.magenta, Color.grey),//assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
                //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
                //uncomment this when you have another skin
            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos, 
                "RalseiMesh");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin

            //creating a new skindef as we did before
            SkinDef masterySkin = Modules.Skins.CreateSkinDef(RALSEI_PREFIX + "MASTERY_SKIN_NAME",
                R2API.Skins.CreateSkinIcon(Color.white, Color.green, Color.green, Color.magenta, Color.grey),//assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                RalseiUnlockables.masterySkinUnlockableDef);

            //adding the mesh replacements as above. 
            //if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                ""/*"RalseiPeeledMesh"*/);

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("RalseiHatless_Base_Color");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            /*masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("GunModel"),
                    shouldActivate = false,
                }
            };*/
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            skins.Add(masterySkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region NikoSkin

            //creating a new skindef as we did before
            SkinDef nikoSkin = Modules.Skins.CreateSkinDef(RALSEI_PREFIX + "NIKO_SKIN_NAME",
                R2API.Skins.CreateSkinIcon(Color.white, Color.green, Color.green, Color.magenta, Color.grey),//assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                RalseiUnlockables.masterySkinUnlockableDef);

            //adding the mesh replacements as above. 
            //if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            nikoSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            nikoSkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("Niko_Base_Color");

            skins.Add(nikoSkin);
            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins


        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            int empowerCount = sender.GetBuffCount(empowerBuff);
            if (empowerCount > 0)
            {
                args.attackSpeedMultAdd += 1 * empowerCount;
                args.moveSpeedMultAdd += 0.5f * empowerCount;
                //args.armorAdd += 100 * empowerCount;
                args.cooldownMultAdd *= Mathf.Pow(0.5f, empowerCount);
                args.baseRegenAdd += 1 * (1 + 0.3f * (sender.level - 1));
                args.armorAdd += 50 * empowerCount;
            }
        }

        public static void EmpowerCharacter(CharacterBody recipient, float duration = 0)
        {
            HealthComponent hc = recipient.healthComponent;
            if(hc != null)
            {
                hc.HealFraction(1, default(ProcChainMask));
            }

            if(duration <= 0)
            {
                recipient.AddBuff(empowerBuff);
            }
            else
            {
                recipient.AddTimedBuff(empowerBuff, duration);
            }
        }
    }
}