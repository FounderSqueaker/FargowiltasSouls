using System;
using FargowiltasSouls.Assets.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class EelDrippler : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureProjectile(ProjectileID.DripplerFlailExtraBall);
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.DripplerFlailExtraBall];
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DripplerFlailExtraBall);
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            DelegateMethods.v3_1 = new Vector3(0.4f, 0f, 0);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity, 5, DelegateMethods.CastLight);

            if (Projectile.ai[0] == 0)
            {
                Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.timeLeft += Main.rand.Next(0, 100);
                Projectile.ai[0] += Main.rand.Next(11);
            }
            Projectile.ai[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;

            float speed = Projectile.timeLeft <= 90 ? 0.6f : 0.6f;
            int divisor = Projectile.timeLeft <= 90 ? 3 : 1;
            if (Projectile.ai[0] % 20 / divisor > 10 / divisor) //donald trump electric chair gif
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, speed);
            else Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.3f, speed);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
                Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 300);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit9 with {Pitch = 0.8f}, Projectile.Center);
            for (int j = 0; j < 4; j++)
            {
                Color color = Color.Lerp(Color.Yellow, Color.DarkRed, 0.9f);
                Particle p = new SmokeParticle(Main.rand.NextVector2FromRectangle(Projectile.Hitbox), 2 * Vector2.UnitX.RotatedByRandom(Math.PI), color, Main.rand.Next(30, 40), Main.rand.NextFloat(0.4f, 0.6f), 0.05f, Main.rand.NextFloat(MathF.Tau), false);
                p.Spawn();
            }
            for (int i = 0; i < 25; i++)
            {
                Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.White, 1.3f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 2f;
                Color glowColor = Color.Red;
                Main.EntitySpriteDraw(t.Value, Projectile.Center + afterimageOffset - Main.screenPosition, null, glowColor, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
