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
				EffectManager.SimpleEffect(Heal.effectPrefab, base.transform.position, Quaternion.identity, true);
			}
			UnityEngine.Object.Destroy(gameObject);
		}
    }
}
