﻿using RalseiMod.Survivors.Ralsei.Achievements;
using RoR2;
using UnityEngine;

namespace RalseiMod.Survivors.Ralsei
{
    public static class RalseiUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                RalseiMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(RalseiMasteryAchievement.identifier),
                RalseiSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}