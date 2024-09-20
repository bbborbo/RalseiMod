using BepInEx;
using RalseiMod.Survivors.Ralsei;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Reflection;
using RalseiMod.Skills;
using System.Linq;
using RoR2.Skills;
using System;
using RalseiMod.Modules;
using RalseiMod.Survivors;
using R2API;
using RalseiMod.Survivors.Ralsei.Components;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RalseiMod
{
    //[BepInDependency(BorboStatUtils.BorboStatUtils.guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.johnedwa.RTAutoSprintEx", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.RiskOfBrainrot.Ror2AggroTools", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(DeployableAPI), nameof(LanguageAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(DamageAPI), nameof(TempVisualEffectAPI))]
    [BepInPlugin(guid, modName, version)]
    public class RalseiPlugin : BaseUnityPlugin
    {
        public const string guid = "com." + teamName + "." + modName;
        public const string teamName = "GodRayProd";
        public const string modName = "RalseiMod";
        public const string version = "0.4.0";

        public const string DEVELOPER_PREFIX = "GRP";

        public static RalseiPlugin instance;
        public static AssetBundle mainAssetBundle;

        #region asset paths
        public const string iconsPath = "";
        #endregion

        #region mods loaded
        public static bool ModLoaded(string modGuid) { return modGuid != "" && BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(modGuid); }
        public static bool autoSprintLoaded => ModLoaded("com.johnedwa.RTAutoSprintEx");
        public static bool iabMissilesLoaded => ModLoaded("com.HouseOfFruits.IAmBecomeMissiles");
        #endregion

        [AutoConfig("Enable Debugging", "Enable debug outputs to the log for troubleshooting purposes. Enabling this will slow down the game.", false)]
        public static bool enableDebugging;

        void Awake()
        {
            instance = this;

            Log.Init(Logger);

            mainAssetBundle = Modules.Assets.LoadAssetBundle("ralseibundle");

            Modules.Config.Init();
            Modules.Language.Init();

            ConfigManager.HandleConfigAttributes(GetType(), "Ralsei", Modules.Config.MyConfig);

            RoR2.TeleporterInteraction.onTeleporterBeginChargingGlobal += WarpMinionsTp;
            On.EntityStates.Missions.BrotherEncounter.Phase1.FixedUpdate += WarpMinionsMithrix;
            On.RoR2.MeridianEventTriggerInteraction.Phase1.FixedUpdate += WarpMinionsSon;

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            //new RalseiSurvivor().Init();
            BeginInitializing<SurvivorBase>(allTypes);
            Modules.Language.TryPrintOutput("RalseiSurvivor.txt");

            BeginInitializing<SkillBase>(allTypes);
            Modules.Language.TryPrintOutput("RalseiSkills.txt");

            RalseiSurvivor.instance.InitializeCharacterMaster();

            // this has to be last
            new Modules.ContentPacks().Initialize();

            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        private void WarpMinionsSon(On.RoR2.MeridianEventTriggerInteraction.Phase1.orig_FixedUpdate orig, MeridianEventTriggerInteraction.Phase1 self)
        {
            if (!self.spawnedEntryFX && self.fixedAge > self.meridianEventTriggerInteraction.additionalEntryVFXDelay + self.durationBeforeEnablingCombatEncounter)
            {
                Vector3 pos = self.meridianEventTriggerInteraction.falseSonEntryFXPosition.position;
                WarpOnTeleporterBegin[] warpTargets = WarpOnTeleporterBegin.GetWarpTargets(pos, 0);
                int count = warpTargets.Length;
                Log.Warning(count);
                int i = 0;
                foreach (WarpOnTeleporterBegin warpTarget in warpTargets)
                {
                    // 7.5 is the magic number to have all turrets on the teleporter platform
                    // needs to be slightly larger for the primordial telepot
                    float Radius = 25f;
                    float radianInc = Mathf.Deg2Rad * 360f / count;
                    Vector3 point1 = new Vector3(Mathf.Cos(radianInc * i) * Radius, 0.25f, Mathf.Sin(radianInc * i) * Radius);

                    i++;

                    var targetFootPos = pos + point1;
                    var turretBody = warpTarget.master.GetBody();

                    TeleportHelper.TeleportBody(turretBody, targetFootPos);
                }
            }
            orig(self);
        }

        private void WarpMinionsMithrix(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_FixedUpdate orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
        {
            if(self.fixedAge + Time.fixedDeltaTime > EntityStates.Missions.BrotherEncounter.Phase1.prespawnSoundDelay && !self.hasPlayedPrespawnSound)
            {
                Vector3 pos = self.childLocator.FindChild("CenterOrbEffect").transform.position;
                WarpOnTeleporterBegin[] warpTargets = WarpOnTeleporterBegin.GetWarpTargets(pos, 0);
                int count = warpTargets.Length;
                Log.Warning(count);
                int i = 0;
                foreach (WarpOnTeleporterBegin warpTarget in warpTargets)
                {
                    // 7.5 is the magic number to have all turrets on the teleporter platform
                    // needs to be slightly larger for the primordial telepot
                    float Radius = 25f;
                    float radianInc = Mathf.Deg2Rad * 360f / count;
                    Vector3 point1 = new Vector3(Mathf.Cos(radianInc * i) * Radius, 0.25f, Mathf.Sin(radianInc * i) * Radius);

                    i++;

                    var targetFootPos = pos + point1;
                    var turretBody = warpTarget.master.GetBody();

                    TeleportHelper.TeleportBody(turretBody, targetFootPos);
                }
            }
            orig(self);
        }

        private void WarpMinionsTp(TeleporterInteraction tp)
        {
            WarpOnTeleporterBegin[] warpTargets = WarpOnTeleporterBegin.GetWarpTargets(tp);
            int count = warpTargets.Length;
            int i = 0;
            foreach (WarpOnTeleporterBegin warpTarget in warpTargets)
            {
                // 7.5 is the magic number to have all turrets on the teleporter platform
                // needs to be slightly larger for the primordial telepot
                float Radius = 8.5f;
                float radianInc = Mathf.Deg2Rad * 360f / count;
                Vector3 point1 = new Vector3(Mathf.Cos(radianInc * i) * Radius, 0.25f, Mathf.Sin(radianInc * i) * Radius);

                i++;

                var targetFootPos = tp.transform.position + point1;
                var turretBody = warpTarget.master.GetBody();

                TeleportHelper.TeleportBody(turretBody, targetFootPos);
            }
        }

        private void BeginInitializing<T>(Type[] allTypes) where T : SharedBase
        {
            Type baseType = typeof(T);
            //base types must be a base and not abstract
            if (!baseType.IsAbstract)
            {
                Log.Error(Log.Combine() + "Incorrect BaseType: " + baseType.Name);
                return;
            }

            Log.Debug(Log.Combine(baseType.Name) + "Initializing");

            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            foreach (var objType in objTypesOfBaseType)
            {
                string s = Log.Combine(baseType.Name, objType.Name);
                Log.Debug(s);
                T obj = (T)System.Activator.CreateInstance(objType);
                if (ValidateBaseType(obj as SharedBase))
                {
                    Log.Debug(s + "Validated");
                    InitializeBaseType(obj as SharedBase);
                    Log.Debug(s + "Initialized");
                }
            }
        }

        bool ValidateBaseType(SharedBase obj)
        {
            return obj.isEnabled;
            /*TypeInfo typeInfo = obj.GetType().GetTypeInfo();
            PropertyInfo isEnabled = typeof(baseType).GetProperties().Where(x => x.Name == nameof(SharedBase.isEnabled)).First();
            if (isEnabled != null && isEnabled.PropertyType == typeof(bool))
            {
                return (bool)isEnabled.GetValue(obj);
            }

            return false;*/
        }
        void InitializeBaseType(SharedBase obj)
        {
            obj.Init();
            /*MethodInfo method = typeof(baseType).GetMethods().Where(x => x.Name == nameof(SharedBase.Init)).First();
            method.Invoke(obj, new object[] { });*/
        }
    }
}