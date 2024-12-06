using EntityStates;
using RalseiMod.Skills.Dummy;
using RalseiMod.Skills.Ralsei;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using RalseiMod.States.Ralsei;

namespace RalseiMod.States.Dummy
{
    class DummyFatigueDeath : RalseiDeathState
    {
        public override void OnEnter()
        {
            Debug.Log("Dummy fatigue burst");
            base.OnEnter();
            DoFatigueBurst();
        }

        private void DoFatigueBurst()
		{
			if (NetworkServer.active)
			{
				List<HealthComponent> list = new List<HealthComponent>();
				SphereSearch sphereSearch = new SphereSearch();
				sphereSearch.radius = Characters.Dummy.deathBlastRadius;
				sphereSearch.origin = base.transform.position;
				sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
				sphereSearch.mask = LayerIndex.entityPrecise.mask;
				sphereSearch.RefreshCandidates();
				sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();

				TeamMask friendly = TeamMask.all;
				friendly.RemoveTeam(characterBody.teamComponent.teamIndex);
				sphereSearch.FilterCandidatesByHurtBoxTeam(friendly);

				HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
				for (int i = 0; i < hurtBoxes.Length; i++)
				{
					HealthComponent healthComponent = hurtBoxes[i].healthComponent;
					if (!list.Contains(healthComponent))
					{
						list.Add(healthComponent);
					}
				}
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = Characters.Dummy.deathDamageCoefficient * characterBody.baseDamage;
				damageInfo.damageType = DamageType.Stun1s;
				damageInfo.procCoefficient = Characters.Dummy.deathProcCoeff;
				damageInfo.attacker = this.gameObject;
                CharacterMaster master = characterBody.masterObject?.GetComponent<CharacterMaster>();
                if (master)
                {
					CharacterMaster ownerMaster = master.minionOwnership?.ownerMaster;
                    if (ownerMaster)
                    {
						GameObject ownerObject = ownerMaster.GetBodyObject();
						if (ownerObject)
							damageInfo.attacker = ownerObject;
                    }
                }
				damageInfo.crit = RollCrit();

                foreach (HealthComponent recipient in list)
				{
					recipient.body.AddTimedBuff(RalseiSurvivor.fatigueDebuff, Characters.Dummy.deathDebuffDuration);
					damageInfo.position = recipient.body.corePosition;
					recipient.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitAll(damageInfo, recipient.gameObject);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, recipient.gameObject);

					/*HealOrb healOrb = new HealOrb();
					healOrb.origin = base.transform.position;
					healOrb.target = recipient.body.mainHurtBox;
					healOrb.healValue = recipient.fullHealth * healFraction;
					healOrb.overrideDuration = 0.1f;
					OrbManager.instance.AddOrb(healOrb);

					recipient.body.AddTimedBuff(RoR2Content.Buffs.CrocoRegen, healDuration);*/
				}
				EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/JellyfishNova") /*HealSpell.loveBombImpact*/
				, new EffectData
				{
					origin = base.transform.position,
					scale = DummyEmpowerBurst.empowerRange
				}, true);
			}
		}
    }
}
