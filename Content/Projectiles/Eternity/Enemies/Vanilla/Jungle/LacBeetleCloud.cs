using FargowiltasSouls.Assets.Textures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Jungle
{
    public class LacBeetleCloud : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.maxPenetrate = -1;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.98f;
            if (Projectile.ai[0] < 60) Projectile.ai[0]++;
            Projectile.Opacity = Projectile.ai[0] / 60;
            Projectile.ai[1] = Projectile.ai[0] * Projectile.width / 10;

            if (Projectile.timeLeft <= 60)
            {
                if (Projectile.timeLeft == 60) Projectile.ai[0] = 0;
                Projectile.Opacity = 1f - (float)(Projectile.ai[0] / 60);
            }
            else
            {
                bool v = Main.rand.NextBool(8);
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, v ? DustID.VenomStaff : DustID.PurpleTorch, Alpha: 100, Scale: 0.8f);
                Main.dust[d].noGravity = true;
            }
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity > 0.4f && Projectile.timeLeft >= 40;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Venom, 60); //its purple so im legally obligated to do this. u get it
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 auraPos = Projectile.Center;
            float radius = 60;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = Projectile.Opacity;

            Vector4 darkColor = Color.Lerp(Color.DarkViolet, Color.Black, 0.5f).ToVector4();
            Vector4 midColor = Color.Lerp(Color.Violet, Color.Black, 0.5f).ToVector4();
            Vector4 lColor = Color.Lerp(Color.Violet, Color.Black, 0.5f).ToVector4();


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
