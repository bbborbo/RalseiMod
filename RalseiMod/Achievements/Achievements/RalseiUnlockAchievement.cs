using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RalseiMod.Achievements
{
    [RegisterAchievement(identifier, unlockableIdentifier, null, 3, null)]
    class RalseiUnlockAchievement : BaseAchievement
    {
        public const string identifier = RalseiSurvivor.RALSEI_PREFIX + "unlockAchievement";
        public const string unlockableIdentifier = RalseiSurvivor.RALSEI_PREFIX + "unlockUnlockable";

        public override void OnInstall()
        {
            base.OnInstall();
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }
    }
}
