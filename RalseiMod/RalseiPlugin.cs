using BepInEx;
using RalseiMod.Survivors.Henry;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace RalseiMod
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
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

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new HenrySurvivor().Initialize();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }
    }
}
