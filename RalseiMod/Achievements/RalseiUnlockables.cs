using R2API;
using RalseiMod.Achievements;
using RoR2;
using UnityEngine;
using static RalseiMod.Modules.Language.Styling;

namespace RalseiMod.Survivors.Ralsei
{
    public static class RalseiUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;
        public static UnlockableDef pacifistUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                RalseiMasteryAchievement.unlockableIdentifier,
                GetAchievementNameToken(RalseiMasteryAchievement.identifier),
                RalseiSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));

            pacifistUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                RalseiPacifistAchievement.unlockableIdentifier,
                GetAchievementNameToken(RalseiPacifistAchievement.identifier),
                RalseiSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
            LanguageAPI.Add("ACHIEVEMENT_RALSEIPACIFIST_NAME", 
                "Ralsei: Pacifist");
            LanguageAPI.Add("ACHIEVEMENT_RALSEIPACIFIST_DESCRIPTION", 
                "As Ralsei, beat the game or obliterate without killing a single non-boss enemy.");
        }
    }
}
