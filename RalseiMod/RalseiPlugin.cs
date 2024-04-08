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

            // character initialization
            new RalseiSurvivor().Init();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();

            Modules.Language.TryPrintOutput("Ralsei.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }
    }
}
