using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Patreon.DanielTheRobot;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Data;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class TwilightStarSpawn : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_79";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 54;
            Projectile.height = 54;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.light = 0.5f;
            Projectile.scale = 0.5f;
            Projectile.timeLeft = 240;
        }

        public override void AI()
        {
            Projectile.velocity *= 1.01f;
            Projectile.rotation = MathHelper.Pi + Projectile.velocity.ToRotation();
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
            base.OnKill(timeLeft);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            new SmallSparkle(target.Center, 2 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), Color.Lerp(Color.SkyBlue, Color.Blue, 0.6f), 1f, 14).Spawn();
            new SmallSparkle(target.Center, 2 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), Color.Lerp(Color.SkyBlue, Color.Blue, 0.6f), 1f, 14).Spawn();
            base.OnHitNPC(target, hit, damageDone);
        }

        #region Drawing
        public float transitToDark;

        private static VertexStrip _vertexStrip = new VertexStrip();

        public override bool PreDraw(ref Color lightColor)
        {
            MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(2f);
            miscShaderData.Apply();
            _vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            Rectangle rectangle = texture2D13.Frame();
            Vector2 origin2 = rectangle.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Color origColor = Color.SkyBlue;

            int count = 6;
            for (int i = 0; i < count; i++)
            {
                float opac = (i + 1) / count;
                Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition, rectangle, 
                    origColor * opac, Projectile.rotation, origin2, opac * Projectile.scale, spriteEffects, 0);
            }
            return false;
        }

        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.White, Color.SkyBlue, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
            result.A /= 2;
            return result;
        }

        private float StripWidth(float progressOnStrip)
        {
            return Projectile.scale * 2 * MathHelper.Lerp(26f, 32f, Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, clamped: true);
        }
        #endregion
    }
}
