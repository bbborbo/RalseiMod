using EntityStates.AffixEarthHealer;
using RalseiMod.Skills;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.Survivors.Ralsei.Components
{
    [RequireComponent(typeof(ProjectileController))]
    class ProjectileHealOnImpact : MonoBehaviour, IProjectileImpactBehavior
    {
		bool done = false;
		public float healFraction => HealSpell.instantHealPercent;
		public float healDuration => HealSpell.healDuration;
		public float healRange => HealSpell.healRange;
		public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
		{
			if (NetworkServer.active && !done)
			{
				done = true;
				List<HealthComponent> list = new List<HealthComponent>();
				SphereSearch sphereSearch = new SphereSearch();
				sphereSearch.radius = healRange;
				sphereSearch.origin = base.transform.position;
				sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
				sphereSearch.mask = LayerIndex.entityPrecise.mask;
				sphereSearch.RefreshCandidates();
				sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();

				TeamMask friendly = new TeamMask();
				friendly.AddTeam(GetComponent<ProjectileController>().teamFilter.teamIndex);
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
					HealOrb healOrb = new HealOrb();
					healOrb.origin = base.transform.position;
					healOrb.target = recipient.body.mainHurtBox;
					healOrb.healValue = recipient.fullHealth * healFraction;
					healOrb.overrideDuration = 0.1f;
					OrbManager.instance.AddOrb(healOrb);

					recipient.body.AddTimedBuff(RoR2Content.Buffs.CrocoRegen, healDuration);
				}
				EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/JellyfishNova") /*HealSpell.loveBombImpact*/
				, new EffectData{
					origin = base.transform.position,
					scale = healRange
				}, true);
			}
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
