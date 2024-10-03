using EntityStates;
using EntityStates.Bandit2;
using RalseiMod.Skills;
using RalseiMod.Survivors.Ralsei;
using RalseiMod.Survivors.Ralsei.Components;
using RoR2;
using RoR2.CharacterAI;
using Ror2AggroTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei.Weapon
{
    class CastPacifySpell : BaseSkillState
    {
		public static List<string> pacifyBodyNameWhitelist = new List<string>() { "UNIDENTIFIED", "AFFIXEARTH_HEALER_BODY_NAME", "URCHINTURRET_BODY_NAME" };
		public static bool IsTargetPacifiable(HurtBox hurtBox)
        {
			return IsCharacterPacifiable(hurtBox.healthComponent?.body);
		}
		public static bool IsCharacterPacifiable(CharacterBody body)
		{
			//null check for arbitration purposes (specifically for the unlock)
			if (body == null)
				return false;

			//whitelisted bodies
			if (pacifyBodyNameWhitelist.Contains(body.baseNameToken) || body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
				return false;

			//if it passes all other checks, its sparable
			return true;
		}
		public static bool IsCharacterSparable(CharacterBody body)
		{
			//null check for arbitration purposes (specifically for the unlock)
			if (body == null)
				return false;

			//players should not be pacifiable under any circumstance
			if (body.isPlayerControlled)
				return false;

			//bosses are not pacifiable
			if (body.isBoss)
			{
				//except umbras
				if (body.inventory?.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
					return true;
				return false;
			}

			//champions shouldnt be pacifiable if the config is false
			if(body.isChampion && !Pacify.championsPacifiable)
            {
				return false;
            }

			//if this body is somehow not a boss or a player but is still immune to executes, they shouldnt be pacifiable either
			if (body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes))
				return false;

			//if it passes all other checks, its pacifiable
			return true;
		}
		public static bool IsCharacterPacifiableAndSparable(CharacterBody body)
		{
			return body && IsCharacterPacifiable(body) && IsCharacterSparable(body);
		}

		public HurtBox target;
		public float baseDuration = 1;
		float duration;
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
			writer.Write(HurtBoxReference.FromHurtBox(target));
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
			target = reader.ReadHurtBoxReference().ResolveHurtBox();
        }
        public override void OnEnter()
        {
			duration = baseDuration / attackSpeedStat;
			PlayAnimation("Gesture, Override", "CastSpellSpecial", "SpellSpecial.playbackRate", duration);

			CastToTargetServer(target);

			base.OnEnter();
		}
        public override void FixedUpdate()
        {
            base.FixedUpdate();
			if(base.fixedAge > duration && base.isAuthority)
            {
				outer.SetNextStateToMain();
            }
        }
        public bool CastToTargetServer(HurtBox hurtBox)
		{
			if (!hurtBox)
				return false;
			HealthComponent victimHealthComponent = hurtBox.healthComponent;
			if (!victimHealthComponent || !victimHealthComponent.alive)
				return false;

			if (NetworkServer.active)
			{
				CharacterBody victimBody = victimHealthComponent.body;
				if (victimBody != null)
				{
					EffectManager.SpawnEffect(StealthMode.smokeBombEffectPrefab, new EffectData
					{
						origin = victimBody.footPosition
					}, true);

					//if the victim is not a boss OR, if they are a boss and they have the umbra item
					//and also require that the victim be not player controlled or immune to executes
					if (IsCharacterSparable(victimBody))
					{
						CharacterBody b = SpareAndRecruitEnemyMinion(hurtBox.healthComponent, victimBody, characterBody);
						if (b)
						{
							if (b.bodyIndex != BodyCatalog.FindBodyIndex("ScavBody") && b.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) <= 0)
							{
								b.inventory.CopyItemsFrom(characterBody.inventory, new Func<ItemIndex, bool>(CopyItemFilter));

								if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.MonsterTeamGainsItems))
									b.inventory.AddItemsFrom(RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.monsterTeamInventory, new Func<ItemIndex, bool>(CopyItemFilter));
							}

							ReplaceMinionAI(b);
							EmpowerAndStunMinion(b);
							//b.inventory.GiveItem(RoR2Content.Items.MinionLeash);

							if (Pacify.swarmsDuplicate && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Swarms))
							{
								CharacterBody a = RespawnEnemyMinion(b.healthComponent, b, characterBody);
								if (a)
								{
									//ReplaceMinionAI(a);
									EmpowerAndStunMinion(a);
									a.inventory.CopyItemsFrom(b.inventory, new Func<ItemIndex, bool>(CopyItemFilter));
								}
							}
						}
						else
						{
							Log.Error("Ralsei Pacify failed to empower its target!");
						}

						return true;
					}

					//if the previous check was false, apply the sleepy buff instead
					victimBody.AddTimedBuff(RalseiSurvivor.fatigueDebuff.buffIndex, Pacify.fatigueDuration);
					Aggro.AggroMinionsToEnemy(characterBody, victimBody, true);
					return true;
				}
			}

			return false;
		}
		private static bool CopyItemFilter(ItemIndex itemIndex)
		{
			if (itemIndex == RoR2Content.Items.AutoCastEquipment.itemIndex)
				return false;
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			return itemDef 
				&& !itemDef.ContainsTag(ItemTag.AIBlacklist) && !itemDef.ContainsTag(ItemTag.BrotherBlacklist) 
				&& !itemDef.ContainsTag(ItemTag.CannotCopy) && !itemDef.ContainsTag(ItemTag.HoldoutZoneRelated) 
				&& (itemDef.tier == ItemTier.Boss || itemDef.tier == ItemTier.Lunar 
				|| itemDef.tier == ItemTier.Tier1 || itemDef.tier == ItemTier.VoidTier1);
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
			if (b.isChampion && b.inventory)
				b.inventory.GiveItem(RoR2Content.Items.HealthDecay, Pacify.championDecayTime);

			b.masterObject.AddComponent<WarpOnTeleporterBegin>();
			b.bodyFlags |= CharacterBody.BodyFlags.ResistantToAOE;
            if (b.inventory)
			{
				//b.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
			}

			CharacterMaster minionMaster = b.master;

			if(b.characterMotor != null)
			{
				AISkillDriver followSkillDriver = minionMaster.gameObject.AddComponent<AISkillDriver>();
				followSkillDriver.customName = "ReturnToLeader";
				followSkillDriver.skillSlot = SkillSlot.None;
				followSkillDriver.minDistance = 80;
				followSkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
				followSkillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
				followSkillDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
				followSkillDriver.shouldSprint = true;
				followSkillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
				followSkillDriver.driverUpdateTimerOverride = 3;
				followSkillDriver.resetCurrentEnemyOnNextDriverSelection = true;

				InsertFollowDriver(minionMaster, followSkillDriver);
			}

			void InsertFollowDriver(CharacterMaster master, AISkillDriver followDriver)
            {
				BaseAI baseAI = master.aiComponents[0];

				List<AISkillDriver> allSkillDrivers = new List<AISkillDriver>();
				allSkillDrivers.Add(followDriver);
				foreach (AISkillDriver skillDriver in baseAI.skillDrivers)
					allSkillDrivers.Add(skillDriver);

				baseAI.skillDrivers = allSkillDrivers.ToArray();
            }
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
                if (body.isBoss)
                {
					//find the squad that this enemy belonged to
					List<CombatSquad> squads = InstanceTracker.GetInstancesList<CombatSquad>();
					foreach (CombatSquad squad in squads)
					{
						int memberIndex = squad.membersList.IndexOf(victimMaster);
						if (memberIndex >= 0)
						{
							//clear boss status and remove them from the healthbar
							squad.RemoveMemberAt(memberIndex);
							//if there are no other enemies remaining in the squad, trigger the defeat of the squad
							if (!squad.defeatedServer && squad.membersList.Count == 0)
							{
								squad.TriggerDefeat();
							}
							break;
						}
					}
                }
				EntityStateMachine[] components = body.GetComponents<EntityStateMachine>();
				foreach (EntityStateMachine obj2 in components)
				{
					obj2.initialStateType = obj2.mainStateType;
				}
			}
			return body;
			void PreSpawnSetup(CharacterMaster newMaster)
			{
				//newMaster.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
				//newMaster.inventory.GiveItem(RoR2Content.Items.Ghost);
				//newMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, duration);
				//newMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, 150);
			}
		}

		public static CharacterBody SpareAndRecruitEnemyMinion(HealthComponent victimHealthComponent, CharacterBody victimBody, CharacterBody ownerBody)
		{
			if (!victimBody || !victimHealthComponent)
			{
				return null;
			}

			//debuffs, buffs, cooldowns, dots, stuns, projectiles
			Util.CleanseBody(victimBody, true, false, true, true, false, false);
			victimBody.master.isBoss = false;

			if(false)//victimBody.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
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
        public override InterruptPriority GetMinimumInterruptPriority()
        {
			return InterruptPriority.Any;
        }
    }
}