using EntityStates;
using EntityStates.AffixEarthHealer;
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
    class DummyEmpowerBurstState : GenericCharacterMain
    {
        public override void OnEnter()
        {
            Debug.Log("Dummy empower burst");
            base.OnEnter();
			Debug.Log("A");
            DoEmpowerBurst();
            outer.SetNextStateToMain();
        }

        private void DoEmpowerBurst()
		{
			Debug.Log("B");
			if (NetworkServer.active)
			{
				Debug.Log("C");
				List<HealthComponent> list = new List<HealthComponent>();
				SphereSearch sphereSearch = new SphereSearch();
				sphereSearch.radius = DummyEmpowerBurst.empowerRange;
				sphereSearch.origin = characterBody.transform.position;
				sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
				sphereSearch.mask = LayerIndex.entityPrecise.mask;
				sphereSearch.RefreshCandidates();
				sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();

				TeamMask friendly = new TeamMask();
				friendly.AddTeam(characterBody.teamComponent.teamIndex);
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
					if (recipient.body.bodyIndex == BodyCatalog.FindBodyIndex(Characters.Dummy.instance.bodyName))
						continue;
					Debug.Log("D");
					recipient.body.AddTimedBuff(RalseiSurvivor.empowerBuff, DummyEmpowerBurst.empowerDuration);
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
