using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Items;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class NymphHeart : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.EmptyTexture;
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.VampireHeal);
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 900;
        }

        public override void AI()
        {
            Player player = FargoSoulsUtil.PlayerExists(Projectile.ai[0]);
            if (!player.Alive())
                return;
            Vector2 dist = player.Center - Projectile.Center;
            dist.Normalize();
            dist *= 2;
            Projectile.velocity = (Projectile.velocity * 15f + dist) / 16f;

            if (Projectile.Distance(player.Center) < 16)
            {
                player.FargoSouls().HealPlayer(20);
                FargoGlobalItem.OnRetrievePickup(player);
                SoundEngine.PlaySound(SoundID.Grab, Projectile.Center);
                Projectile.Kill();
            }
            if (Projectile.timeLeft % 20 == 0 && Projectile.timeLeft != 0)
            {
                Color f = Color.Lerp(Color.Lerp(Color.Red, Color.DeepPink, Main.rand.NextFloat(0.1f, 0.9f)), Color.Transparent, 0.25f);
                Particle p = new HeartParticle(Projectile.Center + Main.rand.NextVector2Circular(10, 10), Projectile.velocity*1.5f, f, 1f, 50, false);
                p.Rotation = p.Velocity.ToRotation();
                p.Spawn();
            }
        }
    }
}