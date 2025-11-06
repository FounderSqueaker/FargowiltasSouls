using Fargowiltas.Content.Projectiles;
using FargowiltasSouls.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class BloodThornWarning : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedStarfish);
                Main.dust[d].scale = 2f;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0;
                Main.dust[d].velocity = new(Projectile.ai[0], Projectile.ai[1]);
            }
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedStarfish);
                Main.dust[d].scale = 2f;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.5f;
            }
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + new Vector2(Projectile.ai[0], Projectile.ai[1]) * 10, 10, DelegateMethods.CastLight);
        }
        public override bool CanHitPlayer(Player target) => false;
        public override void OnKill(int timeLeft)
        {
            Vector2 vel = new(Projectile.ai[0], Projectile.ai[1]);
            int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ProjectileID.SharpTears, Projectile.damage, 0f, Main.myPlayer, 0, Main.rand.NextFloat(0.5f, 1f));
            if (p.IsWithinBounds(Main.maxProjectiles))
            {
                Main.projectile[p].rotation = vel.ToRotation();
            }
        }
    }
}