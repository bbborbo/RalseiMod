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


            AISkillDriver utilityFlee = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            utilityFlee.customName = "UtilityFlee";
            utilityFlee.skillSlot = SkillSlot.Utility;
            utilityFlee.requiredSkill = LiftPrayer.instance.SkillDef;
            utilityFlee.requireSkillReady = true;
            utilityFlee.minDistance = 0;
            utilityFlee.maxDistance = 30;
            utilityFlee.selectionRequiresTargetLoS = true;
            utilityFlee.selectionRequiresOnGround = false;
            utilityFlee.selectionRequiresAimTarget = false;
            utilityFlee.maxTimesSelected = -1;
            //Behavior
            utilityFlee.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            utilityFlee.activationRequiresTargetLoS = false;
            utilityFlee.activationRequiresAimTargetLoS = false;
            utilityFlee.activationRequiresAimConfirmation = false;
            utilityFlee.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            utilityFlee.moveInputScale = 1;
            utilityFlee.aimType = AISkillDriver.AimType.AtMoveTarget;
            utilityFlee.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            utilityFlee.driverUpdateTimerOverride = -1f;

            AISkillDriver fleeEnemy = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            fleeEnemy.customName = "FleeEnemy";
            fleeEnemy.skillSlot = SkillSlot.None;
            fleeEnemy.requireSkillReady = false;
            fleeEnemy.minDistance = 0;
            fleeEnemy.maxDistance = 10;
            //Behavior
            fleeEnemy.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            fleeEnemy.activationRequiresTargetLoS = false;
            fleeEnemy.activationRequiresAimTargetLoS = false;
            fleeEnemy.activationRequiresAimConfirmation = false;
            fleeEnemy.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            fleeEnemy.moveInputScale = 1;
            fleeEnemy.aimType = AISkillDriver.AimType.MoveDirection;
            fleeEnemy.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            fleeEnemy.shouldSprint = true;
            fleeEnemy.driverUpdateTimerOverride = 3;

            AISkillDriver chaseAlly = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            chaseAlly.customName = "ChaseAlly";
            chaseAlly.skillSlot = SkillSlot.None;
            chaseAlly.requireSkillReady = false;
            chaseAlly.minDistance = 50;
            chaseAlly.maxDistance = float.PositiveInfinity;
            chaseAlly.resetCurrentEnemyOnNextDriverSelection = true;
            //Behavior
            chaseAlly.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            chaseAlly.activationRequiresTargetLoS = false;
            chaseAlly.activationRequiresAimTargetLoS = false;
            chaseAlly.activationRequiresAimConfirmation = false;
            chaseAlly.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            chaseAlly.moveInputScale = 1;
            chaseAlly.aimType = AISkillDriver.AimType.MoveDirection;
            chaseAlly.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            chaseAlly.shouldSprint = true;
            chaseAlly.driverUpdateTimerOverride = 3;

            AISkillDriver buffAlly = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            buffAlly.customName = "BuffAlly";
            buffAlly.skillSlot = SkillSlot.Secondary;
            buffAlly.requireSkillReady = true;
            buffAlly.minDistance = 0;
            buffAlly.maxDistance = 50;
            buffAlly.selectionRequiresTargetLoS = false;
            buffAlly.selectionRequiresOnGround = false;
            buffAlly.selectionRequiresAimTarget = false;
            buffAlly.maxTimesSelected = 5;
            //Behavior
            //buffAlly.maxUserHealthFraction = 0.5f;
            buffAlly.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            buffAlly.activationRequiresTargetLoS = false;
            buffAlly.activationRequiresAimTargetLoS = false;
            buffAlly.activationRequiresAimConfirmation = false;
            buffAlly.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            buffAlly.moveInputScale = 0.1f;
            buffAlly.aimType = AISkillDriver.AimType.AtMoveTarget;
            buffAlly.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            buffAlly.driverUpdateTimerOverride = 2f;

            AISkillDriver empowerAlly = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            empowerAlly.customName = "EmpowerAlly";
            empowerAlly.skillSlot = SkillSlot.Special;
            empowerAlly.requireSkillReady = true;
            empowerAlly.minDistance = 0;
            empowerAlly.maxDistance = 50;
            empowerAlly.selectionRequiresTargetLoS = false;
            empowerAlly.selectionRequiresOnGround = false;
            empowerAlly.selectionRequiresAimTarget = false;
            //empowerAlly.maxTimesSelected = 5;
            //Behavior
            empowerAlly.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            //empowerAlly.maxUserHealthFraction = 0.5f;
            empowerAlly.activationRequiresTargetLoS = false;
            empowerAlly.activationRequiresAimTargetLoS = false;
            empowerAlly.activationRequiresAimConfirmation = false;
            empowerAlly.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            empowerAlly.moveInputScale = 0.1f;
            empowerAlly.aimType = AISkillDriver.AimType.AtMoveTarget;
            empowerAlly.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            empowerAlly.driverUpdateTimerOverride = 2;

            AISkillDriver equipmentPursue = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            equipmentPursue.customName = "FireEquipmentAndPursue";
            equipmentPursue.skillSlot = SkillSlot.None;
            equipmentPursue.requireSkillReady = false;
            equipmentPursue.requireEquipmentReady = true;
            equipmentPursue.minDistance = 40;
            equipmentPursue.maxDistance = 120;
            equipmentPursue.selectionRequiresTargetLoS = true;
            equipmentPursue.selectionRequiresOnGround = false;
            equipmentPursue.selectionRequiresAimTarget = false;
            equipmentPursue.maxTimesSelected = -1;
            equipmentPursue.noRepeat = true;
            //Behavior
            equipmentPursue.shouldFireEquipment = true;
            equipmentPursue.maxUserHealthFraction = 1f;
            equipmentPursue.minUserHealthFraction = 0f;
            equipmentPursue.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            equipmentPursue.activationRequiresTargetLoS = true;
            equipmentPursue.activationRequiresAimTargetLoS = false;
            equipmentPursue.activationRequiresAimConfirmation = true;
            equipmentPursue.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            equipmentPursue.moveInputScale = 1;
            equipmentPursue.aimType = AISkillDriver.AimType.AtMoveTarget;
            equipmentPursue.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            equipmentPursue.driverUpdateTimerOverride = -1f;

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

            AISkillDriver chaseDriver = master.AddComponent<AISkillDriver>();
            //Selection Conditions
            chaseDriver.customName = "ChaseEnemy";
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
