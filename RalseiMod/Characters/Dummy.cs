using RalseiMod.Modules;
using RalseiMod.Skills.Dummy;
using RalseiMod.States.Dummy;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.Characters
{
    class Dummy : CharacterBase<Dummy>
    {
        #region config
        public override string ConfigName => CharacterName;

        [AutoConfig("Base Health", "Dummy's base health. 110 is standard for most survivors.", 200f)]
        public static float dummyBaseHealth = 200f;
        [AutoConfig("Base Damage", "Dummy's base damage. 12 is standard for most survivors.", 14f)]
        public static float dummyBaseDamage = 14f;
        [AutoConfig("Base Regen", "Dummy's base regen. 2 is standard for most survivors.", 0f)]
        public static float dummyBaseRegen = 0f;
        [AutoConfig("Base Armor", "Dummy's base armor. 0 is standard for most survivors.", 20)]
        public static float dummyBaseArmor = 20;
        #endregion
        public override string CharacterName => "Dummy";

        public override string CharacterLore => "";

        public override string bodyName => "DummyBody";
        public string masterName => "DummyMaster";

        public override string modelPrefabName => "mdlRalseiDummy";

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyNameToClone = "Commando",
            bodyName = bodyName,
            bodyNameToken = RalseiPlugin.DEVELOPER_PREFIX + "_DUMMY_NAME",
            subtitleNameToken = RalseiPlugin.DEVELOPER_PREFIX + "_DUMMY_SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = new Color32(0, 255, 127, 100),
            sortPosition = 100,

            crosshair = RalseiMod.Modules.Assets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = dummyBaseHealth,
            healthRegen = dummyBaseRegen,
            armor = dummyBaseArmor,
            moveSpeed = 7,
            damage = dummyBaseDamage,

            jumpCount = 1,
            jumpPower = 15
        };
        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
            new CustomRendererInfo
            {
                //uses the name from ChildLocator
                childName = "Model"
            }
        };

        public override BodyIndex bodyIndex { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override void Init()
        {
            base.Init();

            prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreKnockback;
            prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.Mechanical;

            //prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.HasBackstabImmunity;
            prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.ImmuneToLava;
            prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.ResistantToAOE;
            prefabCharacterBody.bodyFlags |= CharacterBody.BodyFlags.OverheatImmune;

            Rigidbody rb = bodyPrefab.GetComponent<Rigidbody>();
            rb.drag *= 3;

            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(RalseiPlugin.DEVELOPER_PREFIX + "_DUMMY_DEFAULT_SKIN_NAME",
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

            skinController.skins = skins.ToArray();
        }

        public override void Hooks()
        {

        }

        public override void InitializeCharacterMaster()
        {
            Debug.LogWarning("Creating DummyMaster");
            GameObject master = Modules.Prefabs.CreateBlankMasterPrefab(bodyPrefab, masterName);

            BaseAI baseAI = master.GetComponent<BaseAI>();
            baseAI.aimVectorDampTime = 0.15f;
            baseAI.aimVectorMaxSpeed = 360;

            AISkillDriver utilityFlee = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            utilityFlee.customName = "UseEmpowerPrimary";
            utilityFlee.skillSlot = SkillSlot.Primary;
            utilityFlee.requiredSkill = null;
            utilityFlee.requireSkillReady = false;
            utilityFlee.minDistance = 0;
            utilityFlee.maxDistance = 10000;
            utilityFlee.selectionRequiresTargetLoS = false;
            utilityFlee.selectionRequiresOnGround = true;
            utilityFlee.selectionRequiresAimTarget = false;
            utilityFlee.maxTimesSelected = -1;
            //Behavior
            utilityFlee.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            utilityFlee.activationRequiresTargetLoS = false;
            utilityFlee.activationRequiresAimTargetLoS = false;
            utilityFlee.activationRequiresAimConfirmation = false;
            utilityFlee.movementType = AISkillDriver.MovementType.Stop;
            utilityFlee.moveInputScale = 1;
            utilityFlee.aimType = AISkillDriver.AimType.None;
            utilityFlee.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            utilityFlee.driverUpdateTimerOverride = -1f;
        }

        public override void InitializeEntityStateMachines()
        {
            Content.AddEntityState(typeof(DummyFatigueDeath));
            CharacterDeathBehavior cdb = bodyPrefab.GetComponent<CharacterDeathBehavior>();
            if (!cdb) 
                cdb = bodyPrefab.AddComponent<CharacterDeathBehavior>();
            cdb.deathState = new EntityStates.SerializableEntityStateType(typeof(DummyFatigueDeath));

            Modules.Prefabs.ClearEntityStateMachines(bodyPrefab);
            Modules.Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.GummyClone.GummyCloneSpawnState));
            //Modules.Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon", typeof(EntityStates.Idle), typeof(EntityStates.Idle));
        }

        public override void Lang()
        {
            Modules.Language.Add(bodyInfo.bodyNameToken, CharacterName);
        }
        public override void InitializeSkills()
        {
            base.InitializeSkills();
            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();
            if (skillLocator != null)
            {
                Modules.Skills.characterSkillLocators[bodyName] = skillLocator;
            }
            //add our own
            //GenericSkill passiveGenericSkill = Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            Modules.Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);
        }
    }
}
