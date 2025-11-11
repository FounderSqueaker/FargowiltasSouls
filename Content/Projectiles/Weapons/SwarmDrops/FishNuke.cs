using System;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class FishNuke : ModProjectile, IPixelatedPrimitiveRenderer
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/SwarmWeapons", Name);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 52;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1800;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            //AIType = ProjectileID.Bullet;
            Projectile.scale = 2f;
        }

        public override void AI()
        {
            if (Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxNPCs)
            {
                int ai0 = (int)Projectile.ai[0];
                if (Main.npc[ai0].CanBeChasedBy())
                {
                    double num4 = (Main.npc[ai0].Center - Projectile.Center).ToRotation() - Projectile.velocity.ToRotation();
                    if (num4 > Math.PI)
                        num4 -= 2.0 * Math.PI;
                    if (num4 < -1.0 * Math.PI)
                        num4 += 2.0 * Math.PI;
                    if (Projectile.velocity != Vector2.Zero)
                        Projectile.velocity = Projectile.velocity.RotatedBy(num4 * 0.1f * Projectile.velocity.Length() / 7f);
                }
                else
                {
                    Projectile.ai[0] = -1f;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                if (++Projectile.localAI[1] > 12f && Projectile.ai[0] != -2)
                {
                    Projectile.localAI[1] = 0f;
                    Projectile.ai[0] = FargoSoulsUtil.FindClosestHostileNPC(Projectile.Center, 16 * 10 + Projectile.width, true);
                    Projectile.netUpdate = true;
                }
            }

            if (++Projectile.localAI[0] >= 24f)
                Projectile.localAI[0] = 0f;
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer && Projectile.width < 400 && Projectile.height < 400 && Projectile.ai[0] != -2f)
            {
                SoundEngine.PlaySound(FargosSoundRegistry.NukeFishronExplosion with { Volume = 0.5f, MaxInstances = 1 }, Projectile.Center);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FishNukeExplosion>(), Projectile.damage, Projectile.knockBack * 2f, Projectile.owner, 0, 0, Main.rand.Next(0, 365));
                Projectile.timeLeft = 20;
                Projectile.hide = true;
                Projectile.ai[0] = -2f;
                Projectile.velocity *= 0;
                Projectile.knockBack *= 2f;
                Projectile.Resize(200, 200);
            }
        }

        /*public override bool? CanHitNPC(NPC target)
        {
            if (!target.noTileCollide && !Collision.CanHitLine(Projectile.Center, 0, 0, target.Center, 0, 0))
                return false;

            return base.CanHitNPC(target);
        }

        epublic override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width /= 2;
            height /= 2;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }*/

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer && Projectile.width < 400 && Projectile.height < 400 && Projectile.ai[0] != -2f)
            {
                SoundEngine.PlaySound(FargosSoundRegistry.NukeFishronExplosion with { Volume = 0.5f, MaxInstances = 1 }, Projectile.Center);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FishNukeExplosion>(), Projectile.damage, Projectile.knockBack * 2f, Projectile.owner, 0, 0, Main.rand.Next(0, 365));
                Projectile.timeLeft = 20;
                Projectile.hide = true;
                Projectile.ai[0] = -2f;
                Projectile.velocity *= 0;
                Projectile.knockBack *= 2f;
                Projectile.Resize(200, 200);
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            int num1 = 36;
            for (int index1 = 0; index1 < num1; ++index1)
            {
                Vector2 vector2_1 = (Projectile.velocity.SafeNormalize(Vector2.UnitY) * new Vector2(50, 50) * 0.75f).RotatedBy((index1 - (num1 / 2 - 1)) * 6.28318548202515 / num1, new Vector2()) + Projectile.Center;
                Vector2 vector2_2 = vector2_1 - Projectile.Center;
                int index2 = Dust.NewDust(vector2_1 + vector2_2, 0, 0, DustID.DungeonWater, vector2_2.X * 2f, vector2_2.Y * 2f, 100, new Color(), 1.4f);
                Main.dust[index2].noGravity = true;
                Main.dust[index2].noLight = true;
                Main.dust[index2].velocity = vector2_2;
            }
        }

        public float WidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * 0.8f;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }

        public Color ColorFunction(float completionRatio)
        {
            float opacity = 1f;
            float threshold = 0.5f;
            if (completionRatio < threshold)
                opacity *= MathF.Pow(completionRatio / threshold, 2);
            return Projectile.GetAlpha(Color.Lerp(Color.DarkOrange, Color.Orange, completionRatio) * opacity);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (Projectile.hide)
                return;
            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.BlobTrail");
            FargoAssets.ColorNoiseMap.Value.SetTexture1();
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader), 25);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects effects = SpriteEffects.None;

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}