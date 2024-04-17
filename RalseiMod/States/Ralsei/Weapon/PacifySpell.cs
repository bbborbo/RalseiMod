using EntityStates.Bandit2;
using RalseiMod.Skills;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.CharacterAI;
using Ror2AggroTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace RalseiMod.States.Ralsei.Weapon
{
    class PacifySpell : EmpowerSpellBaseState
    {
        public override float maxHealthValue => 0.5f;

        public override bool useFriendlyTeam => false;

        public override bool CastToTargetAuthority(HurtBox hurtBox)
        {
			if (!hurtBox)
				return false;
			HealthComponent victimHealthComponent = hurtBox.healthComponent;
			if (!victimHealthComponent || !victimHealthComponent.alive)
				return false;

            CharacterBody victimBody = victimHealthComponent.body;
            if (victimBody)
			{
				EffectManager.SpawnEffect(StealthMode.smokeBombEffectPrefab, new EffectData
				{
					origin = victimBody.footPosition
				}, true);

				//if the victim is not a boss OR, if they are a boss and they have the umbra item
				//and also require that the victim be not player controlled or immune to executes
				if ((!victimBody.isBoss && !victimBody.isPlayerControlled && !victimBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes))
					|| victimBody.inventory?.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
				{
					CharacterBody b = PacifyAndRecruitEnemyMinion(hurtBox.healthComponent, victimBody, characterBody); 
					if (b)
                    {
                        ReplaceMinionAI(b);
						EmpowerAndStunMinion(b);

						if (Pacify.swarmsDuplicate && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Swarms))
						{
							CharacterBody a = RespawnEnemyMinion(hurtBox.healthComponent, victimBody, characterBody);
							if (a)
							{
								ReplaceMinionAI(a);
								EmpowerAndStunMinion(a);
							}
						}
					}
                    else
                    {
						Log.Error("Ralsei Pacify failed to empower its target!");
					}

					SkillLocator skillLocator = characterBody.skillLocator;
					if (skillLocator)
					{
						skillLocator.DeductCooldownFromAllSkillsServer(3f);
					}

                    return true;
                }

				//if the previous check was false, apply the sleepy buff instead
                hurtBox.healthComponent.body.AddTimedBuffAuthority(Pacify.sleepyBuff.buffIndex, 15f);
				return true;
            }

			return false;
		}

        private void EmpowerAndStunMinion(CharacterBody body)
        {
			RalseiSurvivor.EmpowerCharacter(body);

			if (Pacify.convertDelay > 0)
			{
				SetStateOnHurt ssoh = body.GetComponent<SetStateOnHurt>();
				if (ssoh)
				{
					ssoh.SetStun(Pacify.convertDelay);
					body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, Pacify.convertDelay);
					body.AddTimedBuff(RoR2Content.Buffs.Cloak, Pacify.convertDelay);
					body.AddTimedBuff(RoR2Content.Buffs.Nullified, Pacify.convertDelay);
				}
			}
		}

        private static void ReplaceMinionAI(CharacterBody b)
        {
			if(b.inventory)
				b.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
        }

        public static CharacterBody RespawnEnemyMinion(HealthComponent victimHealthComponent, CharacterBody victimBody, CharacterBody ownerBody)
		{
			if (!victimBody || !victimHealthComponent)
			{
				return null;
			}
			//victimBody.AddBuff(Pacify.spareBuff);
			//victimHealthComponent.Suicide();

			GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(victimBody);
			if (!bodyPrefab)
			{
				return null;
			}
			CharacterMaster victimMaster = MasterCatalog.allAiMasters.FirstOrDefault((CharacterMaster master) => master.bodyPrefab == bodyPrefab);
			if (!victimMaster)
			{
				return null;
			}
			MasterSummon summon = new MasterSummon
			{
				masterPrefab = victimMaster.gameObject,
				ignoreTeamMemberLimit = false,
				position = victimBody.footPosition
			};
			CharacterDirection component = victimBody.GetComponent<CharacterDirection>();
			summon.rotation = (component ? Quaternion.Euler(0f, component.yaw, 0f) : victimBody.transform.rotation);
			summon.summonerBodyObject = (ownerBody ? ownerBody.gameObject : null);
			summon.inventoryToCopy = victimBody.inventory;
			summon.useAmbientLevel = Pacify.useAmbientLevel;
			summon.preSpawnSetupCallback = (Action<CharacterMaster>)Delegate.Combine(summon.preSpawnSetupCallback, new Action<CharacterMaster>(PreSpawnSetup));
			CharacterMaster minionMaster = summon.Perform();

			if (!minionMaster)
			{
				return null;
			}

			Deployable deployable = minionMaster.gameObject.AddComponent<Deployable>();
			deployable.onUndeploy = new UnityEvent();
			deployable.onUndeploy.AddListener(new UnityAction(minionMaster.TrueKill));
			ownerBody.master.AddDeployable(deployable, Pacify.pacifyDeployableSlot);

			CharacterBody body = minionMaster.GetBody();
			if ((bool)body)
			{
				EntityStateMachine[] components = body.GetComponents<EntityStateMachine>();
				foreach (EntityStateMachine obj2 in components)
				{
					obj2.initialStateType = obj2.mainStateType;
				}
			}
			return body;
			void PreSpawnSetup(CharacterMaster newMaster)
			{
				newMaster.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
				//newMaster.inventory.GiveItem(RoR2Content.Items.Ghost);
				//newMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, duration);
				//newMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, 150);
			}
		}
		public static CharacterBody PacifyAndRecruitEnemyMinion(HealthComponent victimHealthComponent, CharacterBody victimBody, CharacterBody ownerBody)
		{
			if (!victimBody || !victimHealthComponent)
			{
				return null;
			}

			//debuffs, buffs, cooldowns, dots, stuns, projectiles
			Util.CleanseBody(victimBody, true, false, true, true, false, false);
			if(victimBody.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
				victimBody.inventory.RemoveItem(RoR2Content.Items.InvadingDoppelganger);

			CharacterMaster victimMaster = victimBody.master;
			victimMaster.teamIndex = ownerBody.teamComponent.teamIndex;
			victimBody.teamComponent.teamIndex = ownerBody.teamComponent.teamIndex;
			//victimBody.inventory.SetEquipmentIndex(DLC1Content.Elites.Void.eliteEquipmentDef.equipmentIndex);
			BaseAI aiComponent = victimMaster.GetComponent<BaseAI>();
			if (aiComponent)
			{
				aiComponent.enemyAttention = 0f;
				aiComponent.ForceAcquireNearestEnemyIfNoCurrentEnemy();
				aiComponent.currentEnemy.Reset();
				Aggro.ShedAggroFromCharacter(victimBody);
				//ThisSucks ts = victimMaster.gameObject.AddComponent<ThisSucks>();
				//ts.ai = aiComponent;
				//aiComponent.UpdateTargets();
			}

			AIOwnership ownershipComponent = victimMaster.gameObject.GetComponent<AIOwnership>();
			if (ownershipComponent)
			{
				if (ownerBody.master)
				{
					ownershipComponent.ownerMaster = ownerBody.master;
				}
			}
			if (aiComponent)
			{
				aiComponent.leader.gameObject = ownerBody.gameObject;
			}

			Deployable deployable = victimMaster.gameObject.AddComponent<Deployable>();
			deployable.onUndeploy = new UnityEvent();
			deployable.onUndeploy.AddListener(new UnityAction(victimMaster.TrueKill));
			ownerBody.master.AddDeployable(deployable, Pacify.pacifyDeployableSlot);

			return victimBody;
		}
	}
	public class ThisSucks : MonoBehaviour
    {
		public BaseAI ai;
		void FixedUpdate()
        {
			if (ai == null)
				return;
			if (ai.currentEnemy.gameObject == null)
				return;
			if(ai.currentEnemy.characterBody.teamComponent.teamIndex == ai.body.teamComponent.teamIndex)
            {
				ai.currentEnemy.Reset();
			}
        }
    }
}