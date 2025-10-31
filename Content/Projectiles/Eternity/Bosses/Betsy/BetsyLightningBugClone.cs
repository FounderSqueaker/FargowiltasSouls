using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class BetsyLightningBugClone : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_578";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }


        public override void AI()
        {
            if (Projectile.frameCounter++ > 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 0;
            }

            Projectile owner = Main.projectile[(int)Projectile.ai[0]];
            if (!owner.Alive() || owner.type != ModContent.ProjectileType<BetsyLightningAura>())
            {
                Projectile.Kill();
                return;
            }

            float dist = 800f;
            ref float timer = ref Projectile.ai[2];
            ref float rot = ref Projectile.ai[1];
            float rotTime = 300f;

            timer++;

            Vector2 center = owner.Center;
            Vector2 vel = Vector2.UnitX.RotatedBy(rot + (MathHelper.TwoPi * timer / rotTime));
            Projectile.Center = center + dist * vel;
            Projectile.spriteDirection = -1 * (int)Projectile.HorizontalDirectionTo(center);

            Color sparkColor = Color.Lerp(Color.White, Color.DarkViolet, 0.7f);
            new ElectricSpark(Projectile.Center - 13 * vel.RotatedBy(MathHelper.PiOver2), 0.5f * vel.RotatedBy(MathHelper.PiOver2), sparkColor, 0.8f, 9).Spawn();
            new ExpandingBloomParticle(Projectile.Center, Vector2.Zero, sparkColor, Vector2.One, Vector2.Zero, 15).Spawn();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.Electrified, 120);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_LightningBugDeath, Projectile.Center);

            for (int i = 0; i < 2; i++)
            {
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Main.rand.Next(997, 1000));
            }

            base.OnKill(timeLeft);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 auraPos = Projectile.Center;
            float radius = Math.Min(100f, 1.5f * Projectile.ai[2]);
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = 0.9f * Projectile.Opacity;

            Vector4 darkColor = Color.DarkViolet.ToVector4();
            Vector4 midColor = Color.Black.ToVector4();
            Vector4 lColor = Color.DarkViolet.ToVector4();


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


            FargoSoulsUtil.GenericProjectileDraw(Projectile, Color.Pink);
            return false;
        }
    }
}
