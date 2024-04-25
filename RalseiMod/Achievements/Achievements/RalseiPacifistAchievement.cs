using RalseiMod.States.Ralsei.Weapon;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RalseiMod.Achievements
{
    [RegisterAchievement("RalseiPacifist", unlockableIdentifier, null, null)]
    public class RalseiPacifistAchievement : BaseAchievement
    {
        public const string identifier = RalseiSurvivor.RALSEI_PREFIX + "pacifistAchievement";
        public const string unlockableIdentifier = RalseiSurvivor.RALSEI_PREFIX + "pacifistUnlockable";

        private int killCount = 0;
        public override BodyIndex LookUpRequiredBodyIndex() => BodyCatalog.FindBodyIndex(RalseiSurvivor.instance.bodyName);
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            Run.onRunStartGlobal += ResetKillCount;
            Run.onClientGameOverGlobal += ClearCheck;
            On.RoR2.GlobalEventManager.OnCharacterDeath += KillCheck;
        }

        public override void OnBodyRequirementBroken()
        {
            Run.onRunStartGlobal += ResetKillCount;
            Run.onClientGameOverGlobal -= ClearCheck;
            On.RoR2.GlobalEventManager.OnCharacterDeath -= KillCheck;
            base.OnBodyRequirementBroken();
        }

        private void KillCheck(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if (damageReport.attackerBodyIndex == LookUpRequiredBodyIndex() && damageReport.attackerTeamIndex == TeamIndex.Player 
                && CastPacifySpell.CanCharacterBePacified(damageReport.victimBody))
            {
                if (killCount == 0)
                {
                    Log.Warning("DEBUG: Pacifist challenge failed.");
                    /*if (Base.AnnounceWhenFail.Value)*/
                    Chat.AddMessage("Pacifist challenge failed!");
                }
                ++killCount;
            }
            orig(self, damageReport);
        }

        private void ResetKillCount(Run obj) => killCount = 0;

        public void ClearCheck(Run run, RunReport runReport)
        {
            bool flag = killCount == 0 && meetsBodyRequirement;
            killCount = 0;

            if (!runReport.gameEnding.isWin || !flag || 
                run == null || runReport == null || !(bool)runReport.gameEnding) 
                return;
            Grant();
        }
    }
}
