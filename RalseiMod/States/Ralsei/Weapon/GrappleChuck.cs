using EntityStates;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace RalseiMod.States.Ralsei.Weapon
{
    internal class GrappleChuck : BaseSkillState
    {
        /*public static float BaseDuration = 0.65f;

        public float duration = 0.65f;

        private bool hasFired = false;
        private Ray aimRay;

        private ChildLocator childLoc;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = BaseDuration;
            aimRay = GetAimRay();

            PlayAnimation("FullBody, Override", "PrimaryComboLong", "ScarfPrimary.playbackRate", this.duration);
            //this is only existing so i can do the ammend thing
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!hasFired)
            {
                Fire();
            }

            if (isAuthority && fixedAge >= duration)
                outer.SetNextStateToMain();
        }

        private void Fire()
        {
            hasFired = true;
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();

                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    owner = gameObject,
                    damage = characterBody.damage,
                    projectilePrefab = HenryAssets.grappleLeash,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    speedOverride = 32
                });
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }*/
    }
}