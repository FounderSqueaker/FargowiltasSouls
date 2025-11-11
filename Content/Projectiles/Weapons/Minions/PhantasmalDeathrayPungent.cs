using Fargowiltas.Common.Configs;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.Minions
{
    public class PhantasmalDeathrayPungent : BaseDeathray, IPixelatedPrimitiveRenderer
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Deathrays", "PhantasmalDeathrayWOF");
        public PhantasmalDeathrayPungent() : base(120) { }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            // DisplayName.SetDefault("Divine Deathray");
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Generic;
            //Projectile.Opacity = FargoClientConfig.Instance.TransparentFriendlyProjectiles;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 6;
        }

        public override void AI()
        {
            Projectile.hide = false;
            Vector2? vector78 = null;
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            int byIdentity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, (int)Projectile.ai[0], ModContent.ProjectileType<PungentEyeballMinion>());
            if (byIdentity != -1)
            {
                Projectile.Center = Main.projectile[byIdentity].Center + Vector2.UnitX.RotatedBy(Main.projectile[byIdentity].rotation) * 10f;
            }
            else if (Projectile.owner == Main.myPlayer && Projectile.localAI[0] > 5f)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            if (Projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Pitch = 0.5f }, Projectile.Center);
                SoundEngine.PlaySound(FargosSoundRegistry.GenericDeathray with {Volume = 1.5f}, Projectile.Center);
            }
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }
            Projectile.scale = (float)Math.Sin(Projectile.localAI[0] * 3.14159274f / maxTime) * 2f;
            if (Projectile.scale > 0.5f)
                Projectile.scale = 0.5f;
            Projectile.rotation = Main.projectile[byIdentity].rotation - 1.57079637f;
            Projectile.velocity = Main.projectile[byIdentity].rotation.ToRotationVector2();
            float num805 = 3f;
            float num806 = Projectile.width;
            Vector2 samplingPoint = Projectile.Center;
            if (vector78.HasValue)
            {
                samplingPoint = vector78.Value;
            }
            float[] array3 = new float[(int)num805];
            Collision.LaserScan(samplingPoint, Projectile.velocity, num806 * Projectile.scale, 3000f, array3);
            float num807 = 0f;
            int num3;
            for (int num808 = 0; num808 < array3.Length; num808 = num3 + 1)
            {
                num807 += array3[num808];
                num3 = num808;
            }
            num807 /= num805;
            float amount = 0.5f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], num807, amount);
            Vector2 vector79 = Projectile.Center + Projectile.velocity * (Projectile.localAI[1] - 14f);
            for (int i = 0; i < 10; i++)
            {
                float num810 = Projectile.velocity.ToRotation() + (Main.rand.NextBool(2) ? -1f : 1f) * MathHelper.PiOver2;
                float num811 = (float)Main.rand.NextDouble() * 2f + 2f;
                Vector2 vector80 = new((float)Math.Cos((double)num810) * num811, (float)Math.Sin((double)num810) * num811);
                int num812 = Dust.NewDust(vector79, 0, 0, DustID.ShadowbeamStaff, vector80.X, vector80.Y, 0, default, 1f);
                Main.dust[num812].noGravity = true;
                Main.dust[num812].scale = 2f;
                Main.dust[num812].velocity *= 2f;
                num3 = i;
            }
            DelegateMethods.v3_1 = new Vector3(0.3f, 0.65f, 0.7f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.localAI[1], Projectile.width * Projectile.scale, DelegateMethods.CastLight);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 600);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.DisableCrit();
        }

        public override bool PreDraw(ref Color lightColor) => false;
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.hide)
                behindProjectiles.Add(index);
        }
        public float WidthFunction(float _) => Projectile.width * Projectile.scale * 1f /* FargoClientConfig.Instance.TransparentFriendlyProjectiles*/;

        public static Color ColorFunction(float _)
        {
            Color color = Color.DarkMagenta;
            color.A = 0;
            return color;
        }
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (Projectile.hide)
                return;

            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.LithoDeathray");

            // Get the laser end position.
            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * Projectile.localAI[1];

            // Create 8 points that span across the draw distance from the projectile center.
            Vector2 initialDrawPoint = Projectile.Center - Projectile.velocity * 5f;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(initialDrawPoint, laserEnd, i / (float)(baseDrawPoints.Length - 1f));


            // Set shader parameters.
            shader.TrySetParameter("mainColor", Color.DarkMagenta.ToVector4());
            shader.TrySetParameter("secondColor", Color.White.ToVector4());
            FargoAssets.FadedStreak.Value.SetTexture1();
            shader.TrySetParameter("stretchAmount", 0.25f);
            shader.TrySetParameter("scrollSpeed", 1f);
            shader.TrySetParameter("uColorFadeScaler", 0.8f);
            shader.TrySetParameter("useFadeIn", true);
            shader.TrySetParameter("realopacity", FargoClientConfig.Instance.TransparentFriendlyProjectiles);

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(WidthFunction, ColorFunction, Pixelate: true, Shader: shader), 15);
        }
    }
}