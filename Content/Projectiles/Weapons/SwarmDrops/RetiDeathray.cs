using FargowiltasSouls.Assets.Textures;
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

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class RetiDeathray : BaseDeathray, IPixelatedPrimitiveRenderer
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Deathrays", "PhantasmalDeathray");
        public RetiDeathray() : base(30) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
            CooldownSlot = ImmunityCooldownID.General;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.FargoSouls().CanSplit = false;
            Projectile.tileCollide = false;
            Projectile.scale = 2f;
            Projectile.localNPCHitCooldown = 40;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.hide = true;
            Projectile.extraUpdates = 1;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
        public override bool ShouldUpdatePosition() => false;
        public override void AI()
        {
            Vector2? vector78 = null;
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            int byIdentity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, (int)Projectile.ai[1], ModContent.ProjectileType<Retiglaive>());
            if (byIdentity != -1)
            {
                Projectile.Center = Main.projectile[byIdentity].Center;
            }
            else
            {
                if (Projectile.owner == Main.myPlayer && Projectile.localAI[0] > 5)
                {
                    Projectile.Kill();
                    return;
                }
            }
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            if (Projectile.localAI[0] == 0f)
            {
                if (!Main.dedServ)
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Zombie_104") { Volume = 0.1f }, Projectile.Center);
            }
            float num801 = .3f;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }
            Projectile.scale = (float)Math.Sin(Projectile.localAI[0] * 3.14159274f / maxTime) * 3f * num801;
            if (Projectile.scale > num801)
            {
                Projectile.scale = num801;
            }
            float num805 = 3f;
            float num806 = Projectile.width;
            Vector2 samplingPoint = Projectile.Center;
            if (vector78.HasValue)
            {
                samplingPoint = vector78.Value;
            }
            float[] array3 = new float[(int)num805];
            Collision.LaserScan(samplingPoint, Projectile.velocity, num806 * Projectile.scale, 2000f, array3);
            for (int i = 0; i < array3.Length; i++)
            {
                if (array3[i] > Projectile.ai[2])
                    array3[i] = Projectile.ai[2];
            }
            //for (int i = 0; i < array3.Length; i++) array3[i] = Projectile.localAI[0] * Projectile.ai[1];
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

            Projectile.position -= Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() - 1.57079637f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 600);
        }

        //public override Color? GetAlpha(Color lightColor) => new Color(255, 0, 0) * 0.80f;

        public float WidthFunction(float x)
        {
            float modifier = 1.8f;
            if (x > 0.95f)
                modifier = MathHelper.Lerp(modifier, 0f, (x - 0.95f) / 0.05f);
            return Projectile.width* Projectile.scale * modifier;
        }

        public static Color ColorFunction(float _)
        {
            Color color = Color.DarkRed;
            color.A = 0;
            return color;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (Projectile.hide)
                return;

            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.RetinazerDeathray");

            // Get the laser end position.
            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * Projectile.localAI[1];

            // Create 8 points that span across the draw distance from the projectile center.
            Vector2 initialDrawPoint = Projectile.Center - Projectile.velocity * 20f;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(initialDrawPoint, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Set shader parameters.
            shader.TrySetParameter("mainColor", Color.DarkRed);
            FargoAssets.DeviInnerStreak.Value.SetTexture1();
            shader.TrySetParameter("stretchAmount", 0.5);
            shader.TrySetParameter("scrollSpeed", 4f);
            shader.TrySetParameter("uColorFadeScaler", 0.8f);
            shader.TrySetParameter("useFadeIn", true);
            //shader.SetTexture(FargosTextureRegistry.Techno1Noise.Value, 1, SamplerState.LinearWrap);

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(WidthFunction, ColorFunction, Pixelate: true, Shader: shader), WorldSavingSystem.MasochistModeReal ? 40 : 20);
        }
    }
}