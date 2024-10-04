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

        static string CharacterName => RalseiSurvivor.instance.CharacterName;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                RalseiMasteryAchievement.unlockableIdentifier,
                GetAchievementNameToken(RalseiMasteryAchievement.identifier),
                RalseiSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
            Modules.Language.Add(GetAchievementNameToken(RalseiMasteryAchievement.identifier), $"{CharacterName}: Mastery");
            Modules.Language.Add(GetAchievementDescriptionToken(RalseiMasteryAchievement.identifier), $"As {CharacterName}, beat the game or obliterate on Monsoon.");

            pacifistUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                RalseiPacifistAchievement.unlockableIdentifier,
                GetAchievementNameToken(RalseiPacifistAchievement.identifier),
                RalseiSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
            LanguageAPI.Add(GetAchievementNameToken(RalseiPacifistAchievement.identifier),
                $"{CharacterName}: True Pacifist");
            LanguageAPI.Add(GetAchievementDescriptionToken(RalseiPacifistAchievement.identifier),
                $"As {CharacterName}, beat the game or obliterate without killing a single non-boss enemy.");

            LanguageAPI.Add(GetAchievementNameToken(RalseiUnlockAchievement.identifier), 
                "Pacifist");
            LanguageAPI.Add(GetAchievementDescriptionToken(RalseiUnlockAchievement.identifier), 
                "Beat a stage without killing any non-Boss enemies.");

            LanguageAPI.Add(GetAchievementNameToken(Oops.identifier), 
                $"{CharacterName}: Oops");
            LanguageAPI.Add(GetAchievementDescriptionToken(Oops.identifier), 
                $"As {CharacterName}, die to one of your own minions.");

            LanguageAPI.Add(GetAchievementNameToken(KingHealer.identifier), 
                $"{CharacterName}: King Healer");
            LanguageAPI.Add(GetAchievementDescriptionToken(KingHealer.identifier), 
                $"As {CharacterName}, apply more than 1000000 points of healing in a single run.");

            LanguageAPI.Add(GetAchievementNameToken(PowerfulFriends.identifier), 
                $"{CharacterName}: Powerful Connections");
            LanguageAPI.Add(GetAchievementDescriptionToken(PowerfulFriends.identifier), 
                $"As {CharacterName}, have an allied Scavenger, Umbra, and rare Elite all at once.");
        }
    }
}
