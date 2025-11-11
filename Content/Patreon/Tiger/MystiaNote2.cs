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
    public class MystiaNote2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EighthNote;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120; //
            Projectile.penetrate = 5;
            Projectile.FargoSouls().CanSplit = false;
            Projectile.tileCollide = false;
            Projectile.scale = 0.8f;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
        }

        public override void AI()
        {
            //pulsate
            if (Projectile.localAI[1] == 0)
                Projectile.localAI[1] += Main.rand.Next(60);
            Projectile.scale = 1.1f + 0.1f * (float)Math.Sin(MathHelper.TwoPi / 15 * ++Projectile.localAI[1]);
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
