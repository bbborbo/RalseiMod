using EntityStates;
using RoR2.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RalseiMod.Survivors.Ralsei;

namespace RalseiMod.Skills
{
    class CancelHoverSkill : SkillBase<CancelHoverSkill>
    {
        public override string SkillName => "Cancel Hover State";

        public override string SkillDescription => "Cancel Ralsei\'s hover state.";

        public override string SkillLangTokenName => "CANCELHOVER";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(Idle);

        public override Type BaseSkillDef => typeof(SkillDef);

        //This is not a skill that should be selectable
        public override string CharacterName => "";
        public override SkillSlot SkillSlot => SkillSlot.None;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            interruptPriority = InterruptPriority.PrioritySkill
        };

        public override string ConfigName => "Cancel Hover State";
        public override string ActivationStateMachineName => "";

        public override AssetBundle assetBundle => RalseiPlugin.mainAssetBundle;

        public override void Hooks()
        {

        }
    }
}
