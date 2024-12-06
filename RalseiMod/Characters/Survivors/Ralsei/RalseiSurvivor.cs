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
using static R2API.DamageAPI;
using UnityEngine.Networking;
using RalseiMod.States.Ralsei;

namespace RalseiMod.Survivors.Ralsei
{
    public class RalseiSurvivor : SurvivorBase<RalseiSurvivor>
    {
        #region config
        public override string ConfigName => "Survivor : " + CharacterName;

        [AutoConfig("Jump Count", "Ralsei's base jump count. 1 is standard for most survivors.", 2)]
        public static int ralseiJumpCount = 2;
        [AutoConfig("Jump Power", "Ralsei's jump power. 15 is standard for most survivors.", 19f)]
        public static float ralseiJumpPower = 19f;
        [AutoConfig("Movement Speed", "Ralsei's movement speed. 7 is standard for most survivors.", 8f)]
        public static float ralseiMoveSpeed = 8f;
        [AutoConfig("Base Health", "Ralsei's base health. 110 is standard for most survivors.", 70f)]
        public static float ralseiBaseHealth = 70f;
        [AutoConfig("Base Damage", "Ralsei's base damage. 12 is standard for most survivors.", 14f)]
        public static float ralseiBaseDamage = 14f;
        [AutoConfig("Base Regen", "Ralsei's base regen. 2 is standard for most survivors.", 1f)]
        public static float ralseiBaseRegen = 1f;
        [AutoConfig("Base Armor", "Ralsei's base armor. 0 is standard for most survivors.", 0)]
        public static float ralseiBaseArmor = 0;

        [AutoConfig("Empowerment Armor Bonus", 20)]
        public static int empowerArmor = 20;
        [AutoConfig("Empowerment Attack Speed Multiplier Bonus", 1f)]
        public static float empowerAttackSpeed = 1f;
        [AutoConfig("Empowerment Sprint Speed Multiplier Bonus", 1f)]
        public static float empowerSprintSpeed = 1f;
        [AutoConfig("Empowerment Movement Speed Multiplier Bonus", 0.3f)]
        public static float empowerMoveSpeed = 0.3f;
        [AutoConfig("Empowerment Base Regen Bonus", 2f)]
        public static float empowerRegen = 2f;
        [AutoConfig("Empowerment Cooldown Reduction", 0.5f)]
        public static float empowerCdr = 0.5f;
        [AutoConfig("Stack Empowerment Prefix", 
            "If set to true, characters who are Empowered multiple times will have the Empowered prefix added multiple times, i.e. Empowered Empowered Glacial Jellyfish", true)]
        public static bool stackEmpowerPrefix = true;

        [AutoConfig("Tangle Armor Penalty", 20)]
        public static int tangleArmor = 20;
        [AutoConfig("Tangle Movespeed Penalty", 0.4f)]
        public static float tangleMoveSpeed = 0.4f;

        [AutoConfig("Fatigued Attack Speed Penalty", "How much should Fatigue increase the victim's attack speed reduction stat.", 0.8f)]
        public static float fatigueSpeedPenalty = 0.8f;
        [AutoConfig("Fatigued Armor Penalty", "How much should Fatigue reduce the victim's armor.", 60)]
        public static int fatigueArmorPenalty = 60;
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
        public static string tangleKeywordToken = RalseiPlugin.DEVELOPER_PREFIX + "_KEYWORD_TANGLE";
        public static string empowerKeywordToken = RalseiPlugin.DEVELOPER_PREFIX + "_KEYWORD_EMPOWER";
        public static string sleepKeywordToken = RalseiPlugin.DEVELOPER_PREFIX + "_KEYWORD_SLEEP";
        public static string fatigueKeywordToken = RalseiPlugin.DEVELOPER_PREFIX + "_KEYWORD_FATIGUE";

        public static BuffDef empowerBuff;
        public static GameObject empowerTemporaryEffectPrefabFriendly;
        public static GameObject empowerTemporaryEffectPrefabEnemy;
        public static Material empowerOverlayMaterialFriendly;
        public static Material empowerOverlayMaterialEnemy;

        public static ModdedDamageType TangleOnHit;
        public static BuffDef tangleDebuff;
        public static GameObject tangleTemporaryEffectPrefab;
        public static Material tangleTemporaryEffectMaterial = null;
        public static Material tangleOverlayMaterial;

        public static BuffDef fatigueDebuff;
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

            characterPortrait = assetBundle.LoadAsset<Texture>("texRalseiIcon"),
            bodyColor = new Color32(0,255,127,100),
            sortPosition = 100,

            crosshair = RalseiMod.Modules.Assets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = ralseiBaseHealth,
            healthRegen = ralseiBaseRegen,
            armor = ralseiBaseArmor,
            moveSpeed = ralseiMoveSpeed,
            damage = ralseiBaseDamage,

            jumpCount = ralseiJumpCount,
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


        public override BodyIndex bodyIndex { get; protected set; }
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
            ///     Initializes the body prefab and model,
            ///     State machines,
            ///     Skills,
            ///     Item displays,
            ///     Initializes the display prefab (logbook? css?)
            ///     
            /// Then registers the survivor
            ///
            base.InitializeCharacter();

            TangleOnHit = DamageAPI.ReserveDamageType();
            tangleDebuff = Content.CreateAndAddBuff(
                "RalseiTangle",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ElementalRings/texBuffElementalRingsReadyIcon.tif").WaitForCompletion(),
                Color.magenta, 
                false, 
                true
                );

            tangleTemporaryEffectPrefab = PrefabAPI.InstantiateClone(
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/CrippleEffect.prefab").WaitForCompletion(), "TangleEffect");
            TempVisualEffectAPI.AddTemporaryVisualEffect(tangleTemporaryEffectPrefab, condition => (condition.HasBuff(tangleDebuff) == true), false);

            tangleTemporaryEffectMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/DeathMark/matDeathMarkFire.mat").WaitForCompletion());
            tangleTemporaryEffectMaterial.SetColor("_TintColor", new Color32(172, 80, 155, 255));
            tangleTemporaryEffectMaterial.SetTexture("_BaseTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Treebot/texTreebotRoot3Mask.png").WaitForCompletion());
            tangleTemporaryEffectMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampTritoneSmoothed.png").WaitForCompletion());

            Renderer[] renderers = tangleTemporaryEffectPrefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Renderer smr = renderer;

                if (string.Equals(renderer.name, "Mesh"))
                {
                    renderer.material = tangleTemporaryEffectMaterial;
                }
            }
            ParticleSystem[] particles = tangleTemporaryEffectPrefab.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particleSystem in particles)
            {
                if(string.Equals(particleSystem.name, "Rings"))
                {
                    var main = particleSystem.main;
                    main.startColor = new Color(.82f, 0, .68f, 255); 
                }
                else
                {
                    GameObject.Destroy(particleSystem.gameObject);
                }
            }

            tangleOverlayMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/CritOnUse/matFullCrit.mat"/*"RoR2/Base/Treebot/matWeakEffect.mat"*/).WaitForCompletion());
            tangleOverlayMaterial.SetColor("_TintColor", new Color32(20, 0, 50, 120));
            tangleOverlayMaterial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/DLC1/Common/ColorRamps/texRampConstructLaserTypeB.png").WaitForCompletion());


            empowerBuff = Content.CreateAndAddBuff(
                "RalseiEmpower",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/WardOnLevel/texBuffWarbannerIcon.tif").WaitForCompletion(),
                Color.yellow, 
                true, 
                false
                );


            empowerOverlayMaterialFriendly = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/CritOnUse/matFullCrit.mat").WaitForCompletion());
            empowerOverlayMaterialFriendly.SetColor("_TintColor", new Color32(90, 180, 0, 111)/*(150, 110, 0, 191)*/);
            empowerOverlayMaterialFriendly.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampBanditSplatter.png").WaitForCompletion());

            empowerTemporaryEffectPrefabFriendly = PrefabAPI.InstantiateClone(
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/WardOnLevel/WarbannerBuffEffect.prefab").WaitForCompletion(), "EmpowerInsigniaFriendly");
            TempVisualEffectAPI.AddTemporaryVisualEffect(empowerTemporaryEffectPrefabFriendly, 
                body => (body.HasBuff(empowerBuff) == true && body.teamComponent.teamIndex == TeamIndex.Player), false);
            ParticleSystemRenderer psr = empowerTemporaryEffectPrefabFriendly.GetComponentInChildren<ParticleSystemRenderer>();
            if(psr != null)
            {
                Material newMaterial = UnityEngine.Object.Instantiate(psr.material);
                //newMaterial.SetTexture()
                psr.material = newMaterial;
            }

            empowerOverlayMaterialEnemy = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/CritOnUse/matFullCrit.mat").WaitForCompletion());
            empowerOverlayMaterialEnemy.SetColor("_TintColor", new Color32(180, 0, 90, 111)/*(150, 110, 0, 191)*/);
            empowerOverlayMaterialEnemy.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampBanditSplatter.png").WaitForCompletion());

            empowerTemporaryEffectPrefabEnemy = PrefabAPI.InstantiateClone(
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/WardOnLevel/WarbannerBuffEffect.prefab").WaitForCompletion(), "EmpowerInsigniaEnemy");
            TempVisualEffectAPI.AddTemporaryVisualEffect(empowerTemporaryEffectPrefabEnemy, 
                body => (body.HasBuff(empowerBuff) == true && body.teamComponent.teamIndex != TeamIndex.Player), false);

            LanguageAPI.Add(RALSEI_PREFIX + "EMPOWERED_MODIFIER", "Empowered {0}");


            fatigueDebuff = Content.CreateAndAddBuff(
                "RalseiFatigue", 
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bandit2/texBuffSuperBleedingIcon.tif").WaitForCompletion(), 
                Color.gray, 
                false, 
                true
                );

            HenryAssets.Init(assetBundle);
            HenryBuffs.Init(assetBundle);
            //InitializeCharacterMaster();

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
            Modules.Prefabs.SetupHitBoxGroup(characterModelObject, "ScarfGroup", "SwingHitbox");
            Modules.Prefabs.SetupHitBoxGroup(characterModelObject, "SpinGroup", "SpinHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            Content.AddEntityState(typeof(RalseiDeathState));
            CharacterDeathBehavior cdb = bodyPrefab.GetComponent<CharacterDeathBehavior>();
            if (!cdb)
                cdb = bodyPrefab.AddComponent<CharacterDeathBehavior>();
            cdb.deathState = new EntityStates.SerializableEntityStateType(typeof(RalseiDeathState));

            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Modules.Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Modules.Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Modules.Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
        }
        public override void InitializeCharacterMaster() => RalseiAI.Init(bodyPrefab, masterName);
        public override void InitializeSkills()
        {
            base.InitializeSkills();
            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();
            if (skillLocator != null)
            {
                Modules.Skills.characterSkillLocators[bodyName] = skillLocator;

                #region Achievements
                Modules.Language.Add(RALSEI_PREFIX + "PASSIVE_NAME", $"Tension Points: Arcana");
                Modules.Language.Add(RALSEI_PREFIX + "PASSIVE_DESCRIPTION", 
                    $"{UtilityColor("Blocking")} attacks or {UtilityColor("Tangling")} enemies will {DamageColor("reduce your skill cooldowns")}.");
                #endregion

                //add passive skill
                skillLocator.passiveSkill = new SkillLocator.PassiveSkill
                {
                    enabled = true,
                    skillNameToken = RALSEI_PREFIX + "PASSIVE_NAME",
                    skillDescriptionToken = RALSEI_PREFIX + "PASSIVE_DESCRIPTION",
                    icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
                };
            }
            //add our own
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
            On.RoR2.GlobalEventManager.ProcessHitEnemy += TangleOnHitHook;
        }

        private void TangleOnHitHook(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (!NetworkServer.active)
                return;

            if(damageInfo.procCoefficient > 0 && victim != null)
            {
                if (!damageInfo.rejected)
                {
                    if (damageInfo.HasModdedDamageType(TangleOnHit) && damageInfo.attacker != null)
                    {
                        CharacterBody aBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        CharacterBody vBody = victim.GetComponent<CharacterBody>();
                        if (aBody != null && vBody != null && vBody.healthComponent.alive)
                        {
                            vBody.AddTimedBuff(tangleDebuff.buffIndex, 10f);
                            ApplyRalseiCdr(aBody, 0.5f);
                        }
                    }
                }
                else
                {
                    ApplyRalseiCdr(victim.GetComponent<CharacterBody>(), 0.5f);
                }
            }
        }

        private static void ApplyRalseiCdr(CharacterBody body, float cdr)
        {
            if (body != null && body.bodyIndex == BodyCatalog.FindBodyIndex(RalseiSurvivor.instance.bodyName) || body.bodyIndex == BodyCatalog.FindBodyIndex(Dummy.instance.bodyName))
            {
                SkillLocator skillLocator = body.skillLocator;
                skillLocator.DeductCooldownFromAllSkillsServer(cdr);
            }
        }

        private string EmpowermentNameModifier(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            string name = orig(bodyObject);
            CharacterBody body = bodyObject?.GetComponent<CharacterBody>();
            if(body && body.HasBuff(empowerBuff))
            {
                int buffCount = body.GetBuffCount(empowerBuff);
                for(int i = 0; i < buffCount; i++)
                {
                    name = RoR2.Language.GetStringFormatted(RALSEI_PREFIX + "EMPOWERED_MODIFIER", name);
                    if (stackEmpowerPrefix == false)
                        break;
                }
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

            AddOverlay(empowerOverlayMaterialFriendly, self.body.HasBuff(empowerBuff) && self.body.teamComponent.teamIndex == TeamIndex.Player);
            AddOverlay(empowerOverlayMaterialEnemy, self.body.HasBuff(empowerBuff) && self.body.teamComponent.teamIndex != TeamIndex.Player);
            AddOverlay(tangleOverlayMaterial, self.body.HasBuff(tangleDebuff));

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
                self.radius, self.HasBuff(tangleDebuff), "");
        }

        public override void Lang()
        {
            base.Lang();

            Modules.Language.Add(RALSEI_PREFIX + "DEFAULT_SKIN_NAME", $"Default");
            Modules.Language.Add(RALSEI_PREFIX + "MASTERY_SKIN_NAME", $"Peeled");
            Modules.Language.Add(RALSEI_PREFIX + "NIKO_SKIN_NAME", $"Solstice");

            Modules.Language.Add(tangleKeywordToken, KeywordText("Tangled", 
                $"Reduces armor by {UtilityColor("-" + tangleArmor)} and " +
                $"movement speed by {UtilityColor("-" + ConvertDecimal(tangleMoveSpeed))} for {10} seconds. " +
                $"{DamageColor("Tangled targets are prioritized by your allies")}."));
            Modules.Language.Add(empowerKeywordToken, KeywordText("Empowered", 
                $"Gain +{ConvertDecimal(empowerAttackSpeed)} {DamageColor("attack speed")}, " +
                $"+{ConvertDecimal(empowerMoveSpeed)} {DamageColor("movement speed")}, " +
                $"-{ConvertDecimal(empowerCdr)} {UtilityColor("cooldown reduction")}, " +
                $"+{empowerArmor} {UtilityColor("armor")}, " +
                $"and +{empowerRegen} {HealingColor("base health regeneration per second")}. " +
                $"Can stack."));
            Modules.Language.Add(sleepKeywordToken, KeywordText("Sleeping", 
                $"{DamageColor("Spare")} a non-boss enemy, removing them from the fight " +
                $"{HealthColor("WITHOUT triggering on-kill effects")}. " +
                $"Boss enemies become {UtilityColor("Fatigued")} for {Pacify.fatigueDuration}s instead."));
            Modules.Language.Add(fatigueKeywordToken, KeywordText("Fatigued",
                $"Reduces armor by {UtilityColor("-" + fatigueArmorPenalty)} and " +
                $"attack speed by {UtilityColor("-" + ConvertDecimal(fatigueSpeedPenalty))}."));
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
                R2API.Skins.CreateSkinIcon(Color.white, Color.green, Color.magenta, Color.green, new Color(0,1,0,0.5f)),//assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
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
                R2API.Skins.CreateSkinIcon(Color.white, Color.green, Color.magenta, Color.magenta, Color.grey),//assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                RalseiUnlockables.pacifistUnlockableDef);

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
                args.attackSpeedMultAdd += empowerAttackSpeed * empowerCount;
                args.moveSpeedMultAdd += empowerMoveSpeed * empowerCount;
                args.cooldownMultAdd *= Mathf.Pow(1 - empowerCdr, empowerCount);
                args.baseRegenAdd += empowerRegen * (1 + 0.3f * (sender.level - 1));
                args.armorAdd += empowerArmor * empowerCount;
                if(!sender.isPlayerControlled)
                    args.sprintSpeedAdd += empowerSprintSpeed;
            }
            if (sender.HasBuff(tangleDebuff))
            {
                args.moveSpeedReductionMultAdd += tangleMoveSpeed;
                args.armorAdd -= tangleArmor;
            }
            if (sender.HasBuff(fatigueDebuff))
            {
                args.attackSpeedReductionMultAdd += fatigueSpeedPenalty;
                args.armorAdd -= fatigueArmorPenalty;
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