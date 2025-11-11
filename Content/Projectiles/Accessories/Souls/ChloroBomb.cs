using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class ChloroBomb : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureProjectile(ProjectileID.ChlorophyteOrb);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.ChlorophyteOrb];
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.2f;
            Projectile.scale = 1.3f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
            Projectile.frame = Main.rand.Next(0, Main.projFrames[Type]);
        }

        public override void AI()
        {
            Projectile.velocity *= 0.97f;
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
                Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            NPC target = FargoSoulsUtil.NPCExists(FargoSoulsUtil.FindClosestHostileNPCPrioritizingMinionFocus(Projectile, 1000, true));
            if (target.Alive())
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, 10 * Vector2.UnitX.RotatedBy(Projectile.SafeDirectionTo(target.Center).ToRotation()), ProjectileID.CrystalLeafShot, Projectile.damage, Player.crystalLeafKB, Projectile.owner);
            }

            for (int i = 0; i < 25; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ChlorophyteWeapon, Scale: 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FargoSoulsUtil.GenericProjectileDraw(Projectile, Projectile.GetAlpha(Color.White));
            return false;
        }
    }
}