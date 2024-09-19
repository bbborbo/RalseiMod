using RoR2;
using RalseiMod.Modules.Achievements;
using RalseiMod.Survivors.Ralsei;

namespace RalseiMod.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 5, null)]
    public class RalseiMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = RalseiSurvivor.RALSEI_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = RalseiSurvivor.RALSEI_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => RalseiSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}