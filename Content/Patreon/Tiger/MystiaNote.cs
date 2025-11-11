using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Patreon.Tiger
{
    public  class MystiaNote : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QuarterNote;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180; //
            Projectile.penetrate = 5;
            Projectile.FargoSouls().CanSplit = false;
            Projectile.tileCollide = false;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
        }

        public override void AI()
        {
            //pulsate
            if (Projectile.localAI[1] == 0)
                Projectile.localAI[1] += Main.rand.Next(60);
            Projectile.scale = 1.1f + 0.1f * (float)Math.Sin(MathHelper.TwoPi / 15 * ++Projectile.localAI[1]);

            //send out more notes
            if (Projectile.ai[1]++ > 60)
            {
                Projectile.ai[1] = 0;

                Vector2 velocity = Projectile.velocity.RotatedByRandom(Math.PI / 4);
                velocity *= 0.5f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<MystiaNote2>(), Projectile.damage / 2, 0, Projectile.owner);
                
            }

        }

        public override void OnKill(int timeLeft)
        {
            for (int num627 = 0; num627 < 5; num627++)
            {
                int num628 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 27, 0f, 0f, 80, default(Color), 1.5f);
                Main.dust[num628].noGravity = true;
            }
        }
    }
}
