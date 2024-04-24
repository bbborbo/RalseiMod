using EntityStates;
using EntityStates.Engi.EngiMissilePainter;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    public abstract class EmpowerSpellBaseState : BaseSkillState
	{
		Animator animator;
		public abstract float maxHealthValue { get; }
		public abstract bool useFriendlyTeam { get; }

		public static float stackInterval = 0.125f;
		public static GameObject crosshairOverridePrefab = Paint.crosshairOverridePrefab;
		public static GameObject stickyTargetIndicatorPrefab = Paint.stickyTargetIndicatorPrefab;
		public static string enterSoundString = Paint.enterSoundString;
		public static string exitSoundString = Paint.exitSoundString;
		public static string loopSoundString = Paint.loopSoundString;
		public static string lockOnSoundString = Paint.lockOnSoundString;
		public static string stopLoopSoundString = Paint.stopLoopSoundString;
		public static float maxAngle = Paint.maxAngle * 0.75f;
		public static float maxDistance = Paint.maxDistance;

		private HurtBox currentTarget;
		private EmpowerSpellBaseState.IndicatorInfo currentTargetIndicator;
		private Indicator stickyTargetIndicator;

		private SkillDef confirmTargetDummySkillDef;
		private SkillDef cancelTargetingDummySkillDef;

		private bool releasedKeyOnce;
		private float stackStopwatch;
		private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
		private BullseyeSearch search;
		private bool queuedFiringState;
		private uint loopSoundID;
		private HealthComponent previousHighlightTargetHealthComponent;
		private HurtBox previousHighlightTargetHurtBox;

		public override void OnEnter()
		{
			base.OnEnter();

			if (base.isAuthority)
			{
				//initialize targeting on authority
				this.stickyTargetIndicator = new Indicator(base.gameObject, EmpowerSpellBaseState.stickyTargetIndicatorPrefab);
				this.search = new BullseyeSearch();
			}

			//play animations/sounds
			animator = GetModelAnimator();
			PlayAnimation("Gesture, Override", "PrepareSpellEntry", "SpellSecondary.playbackRate", 0.5f / attackSpeedStat);
			animator.SetBool("spellReady", true);
			Util.PlaySound(EmpowerSpellBaseState.enterSoundString, base.gameObject);
			this.loopSoundID = Util.PlaySound(EmpowerSpellBaseState.loopSoundString, base.gameObject);

			//set crosshair
			if (EmpowerSpellBaseState.crosshairOverridePrefab)
			{
				this.crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, EmpowerSpellBaseState.crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
			}

			//set skill overrides (these skills dont have an activation state, they just stop the original skills from being used temporarily)
			this.confirmTargetDummySkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("EngiConfirmTargetDummy"));
			this.cancelTargetingDummySkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("EngiCancelTargetingDummy"));
			base.skillLocator.primary.SetSkillOverride(this, this.confirmTargetDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
			base.skillLocator.secondary.SetSkillOverride(this, this.cancelTargetingDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}

		public override void OnExit()
		{
			animator.SetBool("spellReady", false);
			if (base.isAuthority && !this.outer.destroying)
			{
				if(!this.queuedFiringState)
				{
					base.activatorSkillSlot.ApplyAmmoPack();
				}
			}
			//unset skill overrides
			base.skillLocator.secondary.UnsetSkillOverride(this, this.cancelTargetingDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
			base.skillLocator.primary.UnsetSkillOverride(this, this.confirmTargetDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);

			//disable target indicators
			if (currentTargetIndicator.indicator != null)
			{
				currentTargetIndicator.indicator.active = false;
			}
			if (this.stickyTargetIndicator != null)
			{
				this.stickyTargetIndicator.active = false;
			}

			//disable crosshair override
			CrosshairUtils.OverrideRequest overrideRequest = this.crosshairOverrideRequest;
			if (overrideRequest != null)
			{
				overrideRequest.Dispose();
			}

			//play sounds/aniamtions
			//base.PlayCrossfade("Gesture, Additive", "ExitHarpoons", 0.1f);
			Util.PlaySound(EmpowerSpellBaseState.exitSoundString, base.gameObject);
			Util.PlaySound(EmpowerSpellBaseState.stopLoopSoundString, base.gameObject);
			base.OnExit();
		}

		private void AddTargetAuthority(HurtBox hurtBox)
		{
			//if an enemy is already targeted, dont re-add them (this is unique from thermal harpoons targeting)
			if (currentTarget == hurtBox)
			{
				return;
			}

			//create new indicator info
			EmpowerSpellBaseState.IndicatorInfo indicatorInfo = new EmpowerSpellBaseState.IndicatorInfo
			{
				indicator = new EmpowerSpellBaseState.RalseiEmpowerIndicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"))
			};
			indicatorInfo.indicator.targetTransform = hurtBox.transform;
			indicatorInfo.indicator.active = true;

			currentTargetIndicator = indicatorInfo;
			currentTarget = hurtBox;
			Util.PlaySound(EmpowerSpellBaseState.lockOnSoundString, base.gameObject);
		}

		public abstract bool CastToTargetAuthority(HurtBox hurtBox);

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.SetAimTimer(3f);
			if (base.isAuthority)
			{
				this.AuthorityFixedUpdate();
			}
		}

		private void AuthorityFixedUpdate()
		{
			HurtBox hurtBox;
			HealthComponent y;
			this.GetCurrentTargetInfo(out hurtBox, out y);

			//while a hurtbox is targeted
			if (hurtBox)
			{
				this.stackStopwatch += Time.fixedDeltaTime;

				//if primary is being held down, and the stack timer is big enough, or primary was just pressed, add the target
				if (base.inputBank.skill1.down && this.stackStopwatch >= EmpowerSpellBaseState.stackInterval)
				{
					this.stackStopwatch = 0f;
					this.AddTargetAuthority(hurtBox);
				}
			}

			//release primary to start firing
			bool m1Released = base.inputBank.skill1.justReleased;
            if (m1Released && currentTarget)
			{
				this.queuedFiringState = true;
				CastToTargetAuthority(currentTarget);
				PlayAnimation("Gesture, Override", "CastSpellSpecial", "SpellSpecial.playbackRate", 1f / attackSpeedStat);
				this.outer.SetNextStateToMain();
				return;
			}
			//cancel target mode immediately - not setting targetModeEnding means it will clear all targets and refund stock
			if (base.inputBank.skill2.justReleased /*|| base.inputBank.skill4.justReleased*/)
			{
				PlayAnimation("Gesture, Override", "PrepareSpellCancel", "SpellSpecial.playbackRate", 0.73f / attackSpeedStat);
				this.outer.SetNextStateToMain();
				return;
			}

			if (hurtBox != this.previousHighlightTargetHurtBox)
			{
				this.previousHighlightTargetHurtBox = hurtBox;
				this.previousHighlightTargetHealthComponent = y;
				this.stickyTargetIndicator.targetTransform = hurtBox.transform;
			}
			this.stickyTargetIndicator.active = this.stickyTargetIndicator.targetTransform;
		}

		private void GetCurrentTargetInfo(out HurtBox currentTargetHurtBox, out HealthComponent currentTargetHealthComponent)
		{
			Ray aimRay = base.GetAimRay();
			this.search.filterByDistinctEntity = true;
			this.search.filterByLoS = true;
			this.search.minDistanceFilter = 0f;
			this.search.maxDistanceFilter = EmpowerSpellBaseState.maxDistance;
			this.search.minAngleFilter = 0f;
			this.search.maxAngleFilter = EmpowerSpellBaseState.maxAngle;
			this.search.viewer = base.characterBody;
			this.search.searchOrigin = aimRay.origin;
			this.search.searchDirection = aimRay.direction;
			this.search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			TeamMask friendly = new TeamMask();
			friendly.AddTeam(base.GetTeam());
			this.search.teamMaskFilter = useFriendlyTeam ? friendly : TeamMask.GetUnprotectedTeams(base.GetTeam());
			this.search.RefreshCandidates();
			this.search.FilterOutGameObject(base.gameObject);
			foreach (HurtBox hurtBox in this.search.GetResults())
			{
				HealthComponent hc = hurtBox.healthComponent;
				if (hc && hc.alive && hc.combinedHealthFraction <= 0.5f && hc.body.master)
				{
					currentTargetHurtBox = hurtBox;
					currentTargetHealthComponent = hurtBox.healthComponent;
					return;
				}
			}
			currentTargetHurtBox = null;
			currentTargetHealthComponent = null;
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        private struct IndicatorInfo
		{
			public EmpowerSpellBaseState.RalseiEmpowerIndicator indicator;
		}

		private class RalseiEmpowerIndicator : Indicator
		{
			public override void UpdateVisualizer()
			{
				base.UpdateVisualizer();
			}

			public RalseiEmpowerIndicator(GameObject owner, GameObject visualizerPrefab) : base(owner, visualizerPrefab)
			{
			}
		}
	}
}
