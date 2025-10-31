using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class BetsyKoboldClone : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_575";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.tileCollide = false;
            Projectile.frame = 2;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.5f;
        }

        Vector2 koboldPos;

        public override void AI()
        {
            Projectile.frame = 1;
            float timeToExplode = WorldSavingSystem.MasochistModeReal ? 60 : 120;
            Projectile.ai[2]++;

            if (Projectile.ai[0] < 0)
            {
                Projectile.active = false;
                return;
            }

            Projectile owner = Main.projectile[(int)Projectile.ai[0]];

            if (!owner.active || owner.type != ModContent.ProjectileType<BetsyFusedSigil>())
            {
                Projectile.active = false;
                return;
            }

            if (Projectile.ai[2] % 2 == 0)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 1f);

            if (Projectile.ai[2] == timeToExplode / 2)
                SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite, Projectile.Center);

            if (Projectile.ai[2] == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite, Projectile.Center);
                Projectile.velocity = Projectile.DirectionTo(owner.Center) * (Projectile.Distance(owner.Center) / timeToExplode);
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 auraPos = Projectile.Center;
            float radius = Projectile.width * 8;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            float timeToExplode = WorldSavingSystem.MasochistModeReal ? 90 : 120;
            var maxOpacity = 0.9f * Projectile.Opacity * (1 * (Projectile.ai[2] - (0f * timeToExplode)) / timeToExplode);

            Vector4 darkColor = Color.Red.ToVector4();
            Vector4 midColor = Color.Orange.ToVector4();
            Vector4 lColor = Color.Yellow.ToVector4();


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


            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }
    }
}