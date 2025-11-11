using System;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Buffs;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.SkyAndRain
{
    public class LightBall : ModProjectile, IPixelatedPrimitiveRenderer
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Enemies/Vanilla/SkyAndRain", "LightBall");
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            }

            Projectile.velocity *= 1f + Math.Abs(Projectile.ai[1]);
            Vector2 acceleration = Projectile.velocity.RotatedBy(Math.PI / 2);
            acceleration *= Projectile.ai[1];
            Projectile.velocity += acceleration;

            Projectile.velocity *= 1 + Projectile.ai[0];

            Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.rotation += 0.1f * Projectile.direction;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

        }

        public override Color? GetAlpha(Color lightColor)
        {
            return base.GetAlpha(lightColor);
            //return new Color(200, 200, 200, 25);
        }

        public float WidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.scale * Projectile.width * 1.3f;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            Color color = Projectile.type == ModContent.ProjectileType<DarkBall>() ? new(182, 27, 248) : new(250, 249, 128);
            return Color.Lerp(color, Color.Transparent, completionRatio) * 0.7f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Color.White, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (Projectile.velocity != Vector2.Zero)
            {
                ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.BlobTrail");
                FargoSoulsUtil.SetTexture1(FargoAssets.FadedStreak.Value);
                PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader), 25);
            }
        }
    }
}