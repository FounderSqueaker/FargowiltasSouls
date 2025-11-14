using FargowiltasSouls.Assets.Textures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class StinkBomb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_681";

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DD2GoblinBomb);
            Projectile.aiStyle = 0;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f * (Projectile.velocity.X/Math.Abs(Projectile.velocity.X));
            Projectile.velocity.Y += 0.2f;
            Projectile.ai[0]++;
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Top.RotatedBy(Projectile.rotation, Projectile.Center), 1, 1, DustID.JungleTorch);
                d.noGravity = true;
                d.scale = 1.5f;
            }
            if (Projectile.ai[0] > 120)
                Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y > 0 && Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.9f;
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            FargoSoulsUtil.DustRing(Projectile.Center, 20, DustID.Smoke, 3, scale: 2);
            for (int i = 0; i < 10; i++)
            {
                Gore g = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.position, 0.1f * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), GoreID.FartCloud1);
                g.rotation = Main.rand.NextFloatDirection();
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<StinkBombProj>(), (int)(0.8f * Projectile.damage), 0f);
            SoundEngine.PlaySound(SoundID.Item16, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
    }

    public class StinkBombProj : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.timeLeft = 180;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] <= 20)
                Projectile.ai[1] = (Projectile.ai[0] * Projectile.width) / 20;
            if (180 - Projectile.ai[0] < 20)
            {
                Projectile.ai[1] = ((Projectile.ai[0] - 140) * Projectile.width) / 20;
                Projectile.Opacity = (180 - Projectile.ai[0]) / 20;
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    Rectangle spawnArea = new Rectangle((int)Projectile.position.X + 10, (int)Projectile.position.Y + 10, Projectile.width - 20, Projectile.height - 20);
                    Dust d = Dust.NewDustDirect(new(spawnArea.X, spawnArea.Y), spawnArea.Width, spawnArea.Height, DustID.FartInAJar);
                    d.velocity *= 0.5f;
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.Poisoned, 180);
            target.AddBuff(BuffID.Stinky, 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 auraPos = Projectile.Center;
            float radius = Projectile.ai[1];
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = Projectile.Opacity;

            Vector4 darkColor = Color.DarkGreen.ToVector4();
            Vector4 midColor = Color.Green.ToVector4();
            Vector4 lColor = Color.Lime.ToVector4();


            ManagedShader borderShader = ShaderManager.GetShader("FargowiltasSouls.NatureExplosionTelegraphShader");
            borderShader.TrySetParameter("darkColor", darkColor);
            borderShader.TrySetParameter("midColor", midColor);
            borderShader.TrySetParameter("lightColor", lColor);
            borderShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            borderShader.TrySetParameter("radius", radius);
            borderShader.TrySetParameter("anchorPoint", auraPos);
            borderShader.TrySetParameter("screenPosition", Main.screenPosition);
            borderShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            borderShader.TrySetParameter("maxOpacity", maxOpacity);

            Main.spriteBatch.GraphicsDevice.Textures[1] = diagonalNoise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, borderShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
