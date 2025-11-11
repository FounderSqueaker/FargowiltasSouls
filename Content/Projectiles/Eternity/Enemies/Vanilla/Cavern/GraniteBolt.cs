using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern
{
    public class GraniteBolt : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureProjectile(ProjectileID.ShadowBeamHostile);
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Vector2.Zero, Scale: 2f * Projectile.scale);
            d.noGravity = true;

            Projectile.velocity *= 1.044f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.Center, 1, 1, DustID.BlueTorch, Scale: Projectile.scale);
            }
            return base.OnTileCollide(oldVelocity);
        }
    }
}
