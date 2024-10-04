using EntityStates;
using EntityStates.RoboBallBoss.Weapon;
using RalseiMod.Skills.Ralsei;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using Ror2AggroTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei.Weapon
{
    class ThrowDummyState : BaseState
    {
        bool dummyThrown = false;

        float prepareDuration;
        float throwDuration;
        public override void OnEnter()
        {
            prepareDuration = ThrowDummySkill.prepareDuration / attackSpeedStat;
            throwDuration = ThrowDummySkill.throwDuration / attackSpeedStat;
            base.OnEnter();
            PlayAnimation("Gesture, Override", "PrepareSpellEntry", "SpellSecondary.playbackRate", prepareDuration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= prepareDuration && !dummyThrown)
            {
                Util.PlaySound(DeployMinions.summonSoundString, base.gameObject);
                PlayAnimation("Gesture, Override", "CastSpellSecondary", "SpellSecondary.playbackRate", 1f / base.attackSpeedStat);
                if (NetworkServer.active)
                {
                    SpawnAndLaunchDummyServer();
                }
                dummyThrown = true;
            }
            if(fixedAge >= prepareDuration + throwDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void SpawnAndLaunchDummyServer()
        {
            if (dummyThrown || !NetworkServer.active)
                return;

            Vector3 position = base.FindModelChild("MuzzleSpell").position;

            CharacterMaster dummyMaster = new MasterSummon
            {
                masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindMasterIndex("DummyMaster")),//RalseiSurvivor.instance.masterName)),//
                position = position,
                rotation = transform.rotation,
                summonerBodyObject = base.gameObject,
                ignoreTeamMemberLimit = true,
                inventoryToCopy = characterBody.master.inventory
            }.Perform();

            if (dummyMaster)
            {
                Deployable deployable = gameObject.AddComponent<Deployable>();
                deployable.onUndeploy = new UnityEvent();
                deployable.onUndeploy.AddListener(new UnityAction(dummyMaster.TrueKill));
                base.characterBody.master.AddDeployable(deployable, ThrowDummySkill.dummyDeployableSlot);
                dummyMaster.onBodyStart += (body) =>
                {
                    if (NetworkServer.active)
                    {
                        Ror2AggroTools.Aggro.ApplyAggroBuff(body, true);
                        Aggro.ShedAggroFromCharacter(characterBody);
                    }
                };

                CharacterBody dummyBody = dummyMaster.GetBody();
                if (dummyBody)
                {
                    Vector3 forceVector = characterBody.inputBank.aimDirection * ThrowDummySkill.throwForce;
                    //dummyMaster.money = (uint)Mathf.FloorToInt(characterBody.master.money);
                    dummyBody.characterMotor.onMotorStart += (body) => 
                    {
                        IPhysMotor component = body.GetComponent<IPhysMotor>();
                        if (component == null)
                        {
                            Debug.LogError("No force info");
                            return;
                        }
                        PhysForceInfo physForceInfo = PhysForceInfo.Create();
                        physForceInfo.force = forceVector;
                        physForceInfo.massIsOne = true;
                        physForceInfo.ignoreGroundStick = true;
                        physForceInfo.disableAirControlUntilCollision = false;
                        component.ApplyForceImpulse(physForceInfo);
                    };
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (dummyThrown)
                return InterruptPriority.Any;
            return InterruptPriority.PrioritySkill;
        }
    }
}
