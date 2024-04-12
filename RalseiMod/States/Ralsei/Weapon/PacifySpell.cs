using EntityStates.Bandit2;
using RalseiMod.Skills;
using RalseiMod.Survivors.Ralsei;
using RoR2;
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

        public override void CastToTargetAuthority(HurtBox hurtBox)
        {
            CharacterBody victimBody = hurtBox.healthComponent?.body;
            if (victimBody)
            {
                if (!victimBody.isBoss && !victimBody.isPlayerControlled && !victimBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes))
                {
                    victimBody.AddBuff(Pacify.spareBuff);
                    hurtBox.healthComponent.Suicide();
					CharacterBody b = TryToCreatePacifiedAlly(victimBody, characterBody); 
					if (b)
					{
						b.AddBuff(RalseiSurvivor.empowerBuff);
						b.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 5f);
					}
                    else
                    {
						Log.Error("Ralsei Pacify failed to empower its target!");
                    }

					EffectManager.SpawnEffect(StealthMode.smokeBombEffectPrefab, new EffectData
                    {
                        origin = victimBody.footPosition
                    }, true);
                    return;
                }
                hurtBox.healthComponent.body.AddTimedBuffAuthority(Pacify.sleepyBuff.buffIndex, 15f);
            }
        }
        public static CharacterBody TryToCreatePacifiedAlly(CharacterBody victimBody, CharacterBody ownerBody)
		{
			if (!victimBody)
			{
				return null;
			}
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
			MasterSummon obj = new MasterSummon
			{
				masterPrefab = victimMaster.gameObject,
				ignoreTeamMemberLimit = false,
				position = victimBody.footPosition
			};
			CharacterDirection component = victimBody.GetComponent<CharacterDirection>();
			obj.rotation = (component ? Quaternion.Euler(0f, component.yaw, 0f) : victimBody.transform.rotation);
			obj.summonerBodyObject = (ownerBody ? ownerBody.gameObject : null);
			obj.inventoryToCopy = victimBody.inventory;
			obj.useAmbientLevel = Pacify.useAmbientLevel;
			obj.preSpawnSetupCallback = (Action<CharacterMaster>)Delegate.Combine(obj.preSpawnSetupCallback, new Action<CharacterMaster>(PreSpawnSetup));
			CharacterMaster characterMaster = obj.Perform();

			if (!characterMaster)
			{
				return null;
			}

			Deployable deployable = victimMaster.gameObject.AddComponent<Deployable>();
			deployable.onUndeploy = new UnityEvent();
			deployable.onUndeploy.AddListener(new UnityAction(characterMaster.TrueKill));
			ownerBody.master.AddDeployable(deployable, DeployableSlot.LoaderPylon);

			CharacterBody body = characterMaster.GetBody();
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
    }
}
