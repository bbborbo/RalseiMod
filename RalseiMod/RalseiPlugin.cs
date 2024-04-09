using BepInEx;
using RalseiMod.Survivors.Ralsei;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Reflection;
using RalseiMod.Modules.Characters;
using RalseiMod.Skills;
using System.Linq;
using RoR2.Skills;
using System;
using RalseiMod.Modules;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RalseiMod
{
    [BepInDependency("com.johnedwa.RTAutoSprintEx", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.HouseOfFruits.IAmBecomeMissiles", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(guid, modName, version)]
    public class RalseiPlugin : BaseUnityPlugin
    {
        public const string guid = "com." + teamName + "." + modName;
        public const string teamName = "GodRayProd";
        public const string modName = "RalseiMod";
        public const string version = "1.0.0";

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

        void Awake()
        {
            instance = this;

            Log.Init(Logger);

            mainAssetBundle = Modules.Assets.LoadAssetBundle("henrybundle");
            Modules.Config.Init();
            Modules.Language.Init();

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            new RalseiSurvivor().Init();
            //BeginInitializing(typeof(SurvivorBase), allTypes);
            Modules.Language.TryPrintOutput("RalseiSurvivor.txt");

            BeginInitializing(typeof(SkillBase), allTypes);
            Modules.Language.TryPrintOutput("RalseiSkills.txt");

            // this has to be last
            new Modules.ContentPacks().Initialize();

            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        private void BeginInitializing(Type baseType, Type[] allTypes)
        {
            //base types must be a base and not abstract
            if (!baseType.IsSubclassOf(typeof(SharedBase)) || !baseType.IsAbstract)
            {
                Log.Error("Aaaahhhhh!!!");
                return;
            }

            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            Log.Debug($"{modName} : {baseType.Name} : Initializing");

            foreach (var objType in objTypesOfBaseType)
            {
                Log.Debug($"{modName} : {baseType.Name} : {objType.Name}");
                object obj = System.Activator.CreateInstance(objType);
                if (ValidateBaseContent(baseType, obj))
                {
                    Log.Debug($"{modName} : {baseType.Name} : {objType.Name} : Validated");
                    InitializeBaseContent(baseType, obj);
                }
            }
        }

        bool ValidateBaseContent(Type baseType, object obj)
        {
            TypeInfo typeInfo = obj.GetType().GetTypeInfo();
            PropertyInfo isEnabled = baseType.GetProperties().Where(x => x.Name == nameof(SharedBase.isEnabled)).First();
            if (isEnabled != null && isEnabled.PropertyType == typeof(bool))
            {
                return (bool)isEnabled.GetValue(obj);
            }

            return false;
        }
        void InitializeBaseContent(Type baseType, object obj)
        {
            MethodInfo method = baseType.GetMethods().Where(x => x.Name == nameof(SharedBase.Init)).First();
            method.Invoke(obj, new object[] { });
            Log.Debug($"{modName} : {baseType.Name} : {obj.GetType().Name} : Initialized");
        }
    }
}