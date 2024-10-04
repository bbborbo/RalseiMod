using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RalseiMod.Achievements
{
    [RegisterAchievement("RalseiOops", unlockableIdentifier, null, 3, null)]
    class Oops : BaseAchievement
    {
        const string name = "Oops";
        public const string identifier = RalseiSurvivor.RALSEI_PREFIX + name + "Achievement";
        public const string unlockableIdentifier = RalseiSurvivor.RALSEI_PREFIX + name + "unlockUnlockable";

    }
}
