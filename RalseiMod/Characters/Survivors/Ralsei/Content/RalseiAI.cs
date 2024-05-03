using RalseiMod.Skills;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace RalseiMod.Survivors.Ralsei
{
    public static class RalseiAI
    {
        public static void Init(GameObject bodyPrefab, string masterName)
        {
            GameObject master = Modules.Prefabs.CreateBlankMasterPrefab(bodyPrefab, masterName);

            BaseAI baseAI = master.GetComponent<BaseAI>();
            baseAI.aimVectorDampTime = 0.15f;
            baseAI.aimVectorMaxSpeed = 360;

            //some fields omitted that aren't commonly changed. will be set to default values

            AISkillDriver equipmentDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            equipmentDriver.customName = "FireEquipmentAndPursue";
            equipmentDriver.skillSlot = SkillSlot.None;
            equipmentDriver.requireSkillReady = false;
            equipmentDriver.requireEquipmentReady = true;
            equipmentDriver.minDistance = 40;
            equipmentDriver.maxDistance = 120;
            equipmentDriver.selectionRequiresTargetLoS = true;
            equipmentDriver.selectionRequiresOnGround = false;
            equipmentDriver.selectionRequiresAimTarget = false;
            equipmentDriver.maxTimesSelected = -1;
            equipmentDriver.noRepeat = true;

            //Behavior
            equipmentDriver.maxUserHealthFraction = 1f;
            equipmentDriver.minUserHealthFraction = 0f;
            equipmentDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            equipmentDriver.activationRequiresTargetLoS = true;
            equipmentDriver.activationRequiresAimTargetLoS = false;
            equipmentDriver.activationRequiresAimConfirmation = true;
            equipmentDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            equipmentDriver.moveInputScale = 1;
            equipmentDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            equipmentDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            equipmentDriver.driverUpdateTimerOverride = -1f;

            AISkillDriver healDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            healDriver.customName = "SecondaryHeal";
            healDriver.skillSlot = SkillSlot.Secondary;
            healDriver.requiredSkill = HealSpell.instance.SkillDef;
            healDriver.requireSkillReady = true;
            healDriver.minDistance = 0;
            healDriver.maxDistance = 60;
            healDriver.selectionRequiresTargetLoS = true;
            healDriver.selectionRequiresOnGround = false;
            healDriver.selectionRequiresAimTarget = true;
            healDriver.maxTimesSelected = 5;

            //Behavior
            healDriver.maxUserHealthFraction = 0.5f;
            healDriver.minTargetHealthFraction = 0.1f;
            healDriver.maxTargetHealthFraction = 0.7f;
            healDriver.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            healDriver.activationRequiresTargetLoS = false;
            healDriver.activationRequiresAimTargetLoS = false;
            healDriver.activationRequiresAimConfirmation = true;
            healDriver.movementType = AISkillDriver.MovementType.Stop;
            healDriver.moveInputScale = 1;
            healDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            healDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            healDriver.driverUpdateTimerOverride = 1f;
            healDriver.noRepeat = true;

            AISkillDriver ascendDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            ascendDriver.customName = "UtilityRise";
            ascendDriver.skillSlot = SkillSlot.Utility;
            ascendDriver.requiredSkill = LiftPrayer.instance.SkillDef;
            ascendDriver.requireSkillReady = true;
            ascendDriver.minDistance = 0;
            ascendDriver.maxDistance = 20;
            ascendDriver.selectionRequiresTargetLoS = true;
            ascendDriver.selectionRequiresOnGround = false;
            ascendDriver.selectionRequiresAimTarget = false;
            ascendDriver.maxTimesSelected = -1;

            //Behavior
            ascendDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            ascendDriver.activationRequiresTargetLoS = false;
            ascendDriver.activationRequiresAimTargetLoS = false;
            ascendDriver.activationRequiresAimConfirmation = false;
            ascendDriver.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            ascendDriver.moveInputScale = 1;
            ascendDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            ascendDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            ascendDriver.driverUpdateTimerOverride = -1f;

            AISkillDriver fleeDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            fleeDriver.customName = "Flee";
            fleeDriver.skillSlot = SkillSlot.None;
            fleeDriver.requireSkillReady = false;
            fleeDriver.minDistance = 0;
            fleeDriver.maxDistance = 10;
            fleeDriver.resetCurrentEnemyOnNextDriverSelection = true;

            //Behavior
            fleeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            fleeDriver.activationRequiresTargetLoS = false;
            fleeDriver.activationRequiresAimTargetLoS = false;
            fleeDriver.activationRequiresAimConfirmation = false;
            fleeDriver.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            fleeDriver.moveInputScale = 1;
            fleeDriver.aimType = AISkillDriver.AimType.MoveDirection;
            fleeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            fleeDriver.shouldSprint = true;
            fleeDriver.driverUpdateTimerOverride = 3;

            //mouse over these fields for tooltips
            AISkillDriver scarfRangeDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            scarfRangeDriver.customName = "PrimaryRange";
            scarfRangeDriver.skillSlot = SkillSlot.Primary;
            scarfRangeDriver.requiredSkill = ScarfRange.instance.SkillDef; //usually used when you have skills that override other skillslots like engi harpoons
            scarfRangeDriver.requireSkillReady = false; //usually false for primaries
            scarfRangeDriver.requireEquipmentReady = false;
            scarfRangeDriver.minUserHealthFraction = float.NegativeInfinity;
            scarfRangeDriver.maxUserHealthFraction = float.PositiveInfinity;
            scarfRangeDriver.minTargetHealthFraction = float.NegativeInfinity;
            scarfRangeDriver.maxTargetHealthFraction = float.PositiveInfinity;
            scarfRangeDriver.minDistance = 10;
            scarfRangeDriver.maxDistance = 140;
            scarfRangeDriver.selectionRequiresTargetLoS = true;
            scarfRangeDriver.selectionRequiresOnGround = false;
            scarfRangeDriver.selectionRequiresAimTarget = true;
            scarfRangeDriver.maxTimesSelected = -1;

            //Behavior
            scarfRangeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            scarfRangeDriver.activationRequiresTargetLoS = true;
            scarfRangeDriver.activationRequiresAimTargetLoS = true;
            scarfRangeDriver.activationRequiresAimConfirmation = true;
            scarfRangeDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            scarfRangeDriver.moveInputScale = 1;
            scarfRangeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            scarfRangeDriver.ignoreNodeGraph = false; //will chase relentlessly but be kind of stupid
            scarfRangeDriver.shouldSprint = false;
            scarfRangeDriver.shouldFireEquipment = false;
            scarfRangeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            //Transition Behavior
            scarfRangeDriver.driverUpdateTimerOverride = 8;
            scarfRangeDriver.resetCurrentEnemyOnNextDriverSelection = true;
            scarfRangeDriver.noRepeat = true;
            scarfRangeDriver.nextHighPriorityOverride = null;

            /*AISkillDriver empowerDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            empowerDriver.customName = "SpecialEmpower";
            empowerDriver.skillSlot = SkillSlot.Special;
            empowerDriver.requireSkillReady = true;
            empowerDriver.minDistance = 0;
            empowerDriver.maxDistance = 20;
            empowerDriver.selectionRequiresTargetLoS = false;
            empowerDriver.selectionRequiresOnGround = false;
            empowerDriver.selectionRequiresAimTarget = false;
            empowerDriver.maxTimesSelected = -1;

            //Behavior
            empowerDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            empowerDriver.activationRequiresTargetLoS = false;
            empowerDriver.activationRequiresAimTargetLoS = false;
            empowerDriver.activationRequiresAimConfirmation = false;
            empowerDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            empowerDriver.moveInputScale = 1;
            empowerDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            empowerDriver.buttonPressType = AISkillDriver.ButtonPressType.Abstain;*/

            AISkillDriver chaseDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            chaseDriver.customName = "Chase";
            chaseDriver.skillSlot = SkillSlot.None;
            chaseDriver.requireSkillReady = false;
            chaseDriver.minDistance = 0;
            chaseDriver.maxDistance = float.PositiveInfinity;

            //Behavior
            chaseDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseDriver.activationRequiresTargetLoS = false;
            chaseDriver.activationRequiresAimTargetLoS = false;
            chaseDriver.activationRequiresAimConfirmation = false;
            chaseDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseDriver.moveInputScale = 1;
            chaseDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            chaseDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            chaseDriver.resetCurrentEnemyOnNextDriverSelection = true;

            //recommend taking these for a spin in game, messing with them in runtimeinspector to get a feel for what they should do at certain ranges and such
        }
    }
}
