using EntityStates;
using RalseiMod.Skills.Dummy;
using RalseiMod.Skills.Ralsei;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.States.Dummy
{
    class DummyFatigueDeath : GenericCharacterDeath
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
				sphereSearch.radius = DummyEmpowerBurst.empowerRange;
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
				foreach (HealthComponent recipient in list)
				{
					recipient.body.AddTimedBuff(RalseiSurvivor.fatigueDebuff, DummyEmpowerBurst.empowerDuration);
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
