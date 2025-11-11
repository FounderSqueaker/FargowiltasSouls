using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Patreon.Tiger
{
    public class RumiaOrb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300; //
            Projectile.penetrate = 8;
            Projectile.FargoSouls().CanSplit = false;
            Projectile.tileCollide = false;
            Projectile.scale = 0.5f;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.alpha = 100;
            Projectile.scale *= 1.005f; 


            const int aislotHomingCooldown = 0;
            const int homingDelay = 60;
            const float desiredFlySpeedInPixelsPerFrame = 5;

            float amountOfFramesToLerpBy = 60; //120 - 33 * modifier; // minimum of 1, please keep in full numbers even though it's a float!

            Projectile.ai[aislotHomingCooldown]++;
            if (Projectile.ai[aislotHomingCooldown] > homingDelay)
            {
                Projectile.ai[aislotHomingCooldown] = homingDelay; //cap this value 

                NPC n = FargoSoulsUtil.NPCExists(FargoSoulsUtil.FindClosestHostileNPC(Projectile.Center, 600, true));
                if (n.Alive())
                {
                    Vector2 desiredVelocity = Projectile.SafeDirectionTo(n.Center) * desiredFlySpeedInPixelsPerFrame;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 1f / amountOfFramesToLerpBy);
                }
            }
        }
    }
}
