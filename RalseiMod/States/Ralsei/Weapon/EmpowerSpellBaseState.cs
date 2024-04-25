using EntityStates;
using EntityStates.Engi.EngiMissilePainter;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RalseiMod.States.Ralsei.Weapon
{
    public abstract class EmpowerSpellBaseState : BaseSkillState
	{
		Animator animator;
		public abstract float maxHealthFraction { get; }
		public abstract bool useFriendlyTeam { get; }
		public abstract GameObject indicatorPrefab { get; }

		public static float stackInterval = 0.125f;
		public static GameObject crosshairOverridePrefab = Paint.crosshairOverridePrefab;
		public static string enterSoundString = Paint.enterSoundString;
		public static string exitSoundString = Paint.exitSoundString;
		public static string loopSoundString = Paint.loopSoundString;
		public static string lockOnSoundString = Paint.lockOnSoundString;
		public static string stopLoopSoundString = Paint.stopLoopSoundString;
		public static float maxAngle = Paint.maxAngle * 0.75f;
		public static float maxDistance = Paint.maxDistance;

		private HurtBox currentTarget;
		private Indicator targetIndicator;

		private SkillDef confirmTargetDummySkillDef;
		private SkillDef cancelTargetingDummySkillDef;

		private float stackStopwatch;
		private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
		private BullseyeSearch targetFinder;
		private bool queuedFiringState;
		private uint loopSoundID;

		bool releasedKeyOnce = false;

		public override void OnEnter()
        {
            base.OnEnter();

            this.targetFinder = new BullseyeSearch();
            ConfigureTargetFinder();

            //play animations/sounds
            animator = GetModelAnimator();
            PlayAnimation("Gesture, Override", "PrepareSpellEntry", "SpellSecondary.playbackRate", 0.5f / attackSpeedStat);
            animator.SetBool("spellReady", true);

            Util.PlaySound(EmpowerSpellBaseState.enterSoundString, base.gameObject);
            this.loopSoundID = Util.PlaySound(EmpowerSpellBaseState.loopSoundString, base.gameObject);

            //set crosshair
            if (EmpowerSpellBaseState.crosshairOverridePrefab)
            {
                this.crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, 
					EmpowerSpellBaseState.crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
			}
			this.targetIndicator = new Indicator(base.gameObject, indicatorPrefab);

			//set skill overrides (these skills dont have an activation state, they just stop the original skills from being used temporarily)
			this.confirmTargetDummySkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("EngiConfirmTargetDummy"));
            this.cancelTargetingDummySkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("EngiCancelTargetingDummy"));
            base.skillLocator.primary.SetSkillOverride(this, this.confirmTargetDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.secondary.SetSkillOverride(this, this.cancelTargetingDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void ConfigureTargetFinder()
        {
            this.targetFinder.filterByDistinctEntity = true;
            this.targetFinder.filterByLoS = true;
            this.targetFinder.minDistanceFilter = 0f;
            this.targetFinder.maxDistanceFilter = EmpowerSpellBaseState.maxDistance;
            this.targetFinder.minAngleFilter = 0f;
            this.targetFinder.maxAngleFilter = EmpowerSpellBaseState.maxAngle;
            this.targetFinder.viewer = base.characterBody;
            this.targetFinder.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;

			if (useFriendlyTeam)
			{
				this.targetFinder.teamMaskFilter = TeamMask.none;
				this.targetFinder.teamMaskFilter.AddTeam(base.GetTeam());
			}
			else
			{
				this.targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(base.GetTeam());
			}
        }

        private HurtBox GetCurrentTargetInfo()
		{
			Ray aimRay = base.GetAimRay();
			this.targetFinder.searchOrigin = aimRay.origin;
			this.targetFinder.searchDirection = aimRay.direction;
			this.targetFinder.RefreshCandidates();
			this.targetFinder.FilterOutGameObject(base.gameObject);

			using (IEnumerator<HurtBox> enumerator = this.targetFinder.GetResults().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					HurtBox hurtBox = enumerator.Current;
					HealthComponent hc = hurtBox.healthComponent;
					if (hc && hc.alive && hc.combinedHealthFraction <= maxHealthFraction && hc.body.master)
					{
						return hurtBox;
					}
				}
			}
			return null;
		}

		public override void OnExit()
		{
			//play sounds/animations
			animator.SetBool("spellReady", false);
			Util.PlaySound(EmpowerSpellBaseState.exitSoundString, base.gameObject);
			Util.PlaySound(EmpowerSpellBaseState.stopLoopSoundString, base.gameObject);

			if (this.queuedFiringState)
			{
				PlayAnimation("Gesture, Override", "CastSpellSpecial", "SpellSpecial.playbackRate", 1f / attackSpeedStat);
				if (NetworkServer.active)
				{
					CastToTargetServer(currentTarget);
				}
            }
            else
			{
				PlayCrossfade("Gesture, Override", "PrepareSpellCancel", "SpellSpecial.playbackRate", 0.73f / attackSpeedStat, 0.1f / attackSpeedStat);
				if (base.isAuthority && !this.outer.destroying)
				{
					//this needs to be in OnExit in case the skill is canceled by sprinting
					base.activatorSkillSlot.ApplyAmmoPack();
				}
			}
			//unset skill overrides
			base.skillLocator.secondary.UnsetSkillOverride(this, this.cancelTargetingDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
			base.skillLocator.primary.UnsetSkillOverride(this, this.confirmTargetDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);

			//disable crosshair override
			CrosshairUtils.OverrideRequest overrideRequest = this.crosshairOverrideRequest;
			if (overrideRequest != null)
			{
				overrideRequest.Dispose();
			}

			//unset the target, clearing the indicator
			SetTarget(null);

			base.OnExit();
		}

		public abstract bool CastToTargetServer(HurtBox hurtBox);

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				this.AuthorityFixedUpdate();
			}
		}
        public override void Update()
        {
            base.Update();

            SetTarget(GetCurrentTargetInfo());

			return;
            if (base.isAuthority && currentTarget != null && base.inputBank.skill1.justReleased)
			{
				//CastToTargetServer(currentTarget);
				this.queuedFiringState = true;
                this.outer.SetNextStateToMain();
            }
        }

        private void SetTarget(HurtBox hb)
        {
            if (currentTarget != hb)
            {
                currentTarget = hb;

                bool targetAvailable = (currentTarget != null);
                this.targetIndicator.active = targetAvailable;
                this.targetIndicator.targetTransform = targetAvailable ? currentTarget.transform : null;
                if (hb != null)
                    Util.PlaySound(EmpowerSpellBaseState.lockOnSoundString, base.gameObject);
            }
        }

        private void AuthorityFixedUpdate()
		{
			bool skill1Released = base.inputBank.skill1.justReleased;
			bool skill2Released = base.inputBank.skill2.justReleased;
			bool skill4Released = base.inputBank.skill4.justReleased;

			bool tryFiring = false;
			//release primary to start firing
			if (skill1Released)
			{
				tryFiring = true;
			}
			//release special to start firing. the first release must be ignored, otherwise the state will end instantly after pressing the special button to enter
			if (skill4Released)
            {
				if(releasedKeyOnce)
					tryFiring = true;

				releasedKeyOnce = true;
            }

			//if a valid primary or special input was made, and the caster has a target, then cast the spell and exit
			if (tryFiring && currentTarget != null)
			{
				//CastToTargetServer(currentTarget);
				this.queuedFiringState = true;
				this.outer.SetNextStateToMain();
				return;
			}

			//if a fire attempt failed, or if m2 was pressed, then cancel the spell and exit
			if (tryFiring || skill2Released)
			{
				this.queuedFiringState = true;
				this.outer.SetNextStateToMain();
			}
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
	}
}
