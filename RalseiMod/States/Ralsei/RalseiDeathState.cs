using EntityStates;
using RalseiMod.Survivors.Ralsei;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RalseiMod.States.Ralsei
{
    class RalseiDeathState : GenericCharacterDeath
    {

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void CreateDeathEffects()
        {
            if (!this.isBrittle)
            {
                EffectManager.SimpleEffect(HenryAssets.sillyExplosionEffect, transform.position, transform.rotation, false);
            }
        }
        public override void PlayDeathAnimation(float crossfadeDuration = 0.1F)
        {
            base.DestroyModel();
        }
        public override void PlayDeathSound()
        {
            if (!this.isBrittle)
            {
                AkSoundEngine.PostEvent(/*535854868*/AkSoundEngine.GetIDFromString("SillyExplosionSound"), gameObject);
            }
        }
    }
}
