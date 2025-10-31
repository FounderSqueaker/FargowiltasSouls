using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    internal class BetsyLightningAura : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 800;
            Projectile.height = 800;
            Projectile.tileCollide = false;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float dist = targetHitbox.Distance(Projectile.Center);

            if (dist > 2000)
                return false;

            if (dist > 800)
                return true;

            if (Projectile.ai[1] == 1 && Projectile.ai[2] > 180)
            {
                float rotToBetsy = Projectile.Center.AngleTo(Main.npc[(int)Projectile.ai[0]].Center);
                float rotToPlayer = Projectile.Center.AngleTo(targetHitbox.Center());
                if (Math.Abs(rotToBetsy - rotToPlayer) > (MathHelper.PiOver4 / 2f))
                {
                    return true;
                }
            }

            return false;
        }

        public override void AI()
        {
            ref float timer = ref Projectile.ai[2];

            NPC betsy = Main.npc[(int)Projectile.ai[0]];
            if (!betsy.Alive() || betsy.type != NPCID.DD2Betsy)
            {
                Projectile.Kill();
                return;
            }

            int timeToDet = 120;
            if (Projectile.ai[1] == 1)
            {
                timer++;
                if (timer == timeToDet)
                {
                    foreach (var proj in Main.ActiveProjectiles)
                    {
                        if (proj.type == ModContent.ProjectileType<BetsyDarkMageClone>() || proj.type == ModContent.ProjectileType<BetsyLightningBugClone>())
                            proj.Kill();
                    }
                }

                if (timer > timeToDet)
                {
                    float safeRot = MathHelper.PiOver4;
                    int count = 50;
                    if (timer % 3 == 1)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            float rot = Projectile.AngleTo(betsy.Center) + (safeRot / 2) + Main.rand.NextFloat(0f, MathHelper.TwoPi - (safeRot));
                            float scale = Main._rand.NextFloat(0f, 3f);
                            new ElectricSpark(Projectile.Center, scale * 25 * Vector2.UnitX.RotatedBy(rot), Color.Purple, scale, 90).Spawn();
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            float rot = Projectile.AngleTo(betsy.Center) - (safeRot / 2) + Main.rand.NextFloat(0f, safeRot);
                            float scale = Main._rand.NextFloat(0f, 2f);
                            new ElectricSpark(Projectile.Center, scale * 8 * Vector2.UnitX.RotatedBy(rot), Color.Purple, scale, 90).Spawn();
                        }
                    }
                    
                    if (timer % 2 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f, Volume = 2f }, Projectile.Center);
                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f, Volume = 2f }, Projectile.Center);
                    }

                }

                if (timer > 300)
                {
                    Projectile.Kill();
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.Electrified, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 auraPos = Projectile.Center;
            float radius = Projectile.width * 1.5f;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.CracksNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            float opac = Projectile.ai[0] <= 90 ? 0 : MathHelper.Min(1, (Projectile.ai[0] - 90f) / 120f);
            var maxOpacity = 0.5f * Projectile.Opacity * opac;

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

            return false;
        }
    }
}
