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
        #endregion

        void Awake()
        {
            instance = this;

            Log.Init(Logger);

            mainAssetBundle = Modules.Assets.LoadAssetBundle("myassetbundle");
            Modules.Config.Init();
            Modules.Language.Init();

            Type[] allTypes = typeof(RalseiPlugin).Assembly.GetTypes();

            //new RalseiSurvivor().Init();
            BeginInitializing(typeof(SurvivorBase<>), allTypes);
            BeginInitializing(typeof(SkillBase<>), allTypes);
            ScarfRange.instance.KeywordTokens[0] = "";

            // this has to be last
            new Modules.ContentPacks().Initialize();

            Modules.Language.TryPrintOutput("Ralsei.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        private void BeginInitializing(Type baseType, Type[] allTypes)
        {
            //filter out non-bases
            if (!baseType.IsSubclassOf(typeof(SharedBase<>)) || !baseType.IsAbstract)
            {
                Log.Error("Aaaahhhhh!!!");
                return;
            }

            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            Log.Info($"{modName} : {nameof(baseType)} : Initializing");

            foreach (Type objType in objTypesOfBaseType)
            {
                object obj = System.Activator.CreateInstance(objType);

                if (ValidateBaseContent(baseType, obj))
                {
                    InitializeBaseContent(baseType, obj);
                }
            }
        }

        bool ValidateBaseContent(Type baseType, object obj)
        {
            TypeInfo typeInfo = obj.GetType().GetTypeInfo();
            FieldInfo isEnabled = baseType.GetFields().Where(x => x.Name == "isEnabled").First();
            if(isEnabled != null && isEnabled.FieldType == typeof(bool))
            {
                return (bool)isEnabled.GetValue(obj);
            }

            return false;
        }
        void InitializeBaseContent(Type baseType, object obj)
        {
            MethodInfo method = baseType.GetMethods().Where(x => x.Name == "Init").First();
            method = method.MakeGenericMethod(obj.GetType());
            method.Invoke(obj, new object[] { });
        }
    }
}