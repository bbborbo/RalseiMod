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
    class ProjectileBuffOnImpact : MonoBehaviour, IProjectileImpactBehavior
    {
		public BuffDef buffDef => ProtectSpell.blockBuff;
		public float buffDuration => ProtectSpell.blockDuration;
		public float buffRange => ProtectSpell.effectRange;
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
			if (NetworkServer.active)
			{
				List<HealthComponent> list = new List<HealthComponent>();
				SphereSearch sphereSearch = new SphereSearch();
				sphereSearch.radius = buffRange;
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
					healOrb.healValue = 0;
					healOrb.overrideDuration = 0.1f;
					OrbManager.instance.AddOrb(healOrb);

					recipient.body.AddTimedBuff(buffDef, buffDuration);
				}
				EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/JellyfishNova")//Resources.Load<GameObject>("prefabs/effects/JellyfishNova") /*HealSpell.loveBombImpact*/
				, new EffectData
				{
					origin = base.transform.position,
					scale = buffRange
				}, true);
				//EffectManager.SimpleEffect(Heal.effectPrefab, base.transform.position, Quaternion.identity, true);
			}
			UnityEngine.Object.Destroy(gameObject);
		}
    }
}
