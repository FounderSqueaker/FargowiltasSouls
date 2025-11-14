using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
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

            int timeToDet = WorldSavingSystem.MasochistModeReal ? 120 : 180;
            if (Projectile.ai[1] == 1)
            {
                if (timer < 0)
                    timer = 0;

                if (timer == 0)
                {
                    Vector2 pos = betsy.Center + 270 * Vector2.UnitX.RotatedBy(betsy.AngleTo(Projectile.Center));
                    SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost with { Pitch = -0.5f }, pos);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(Projectile.InheritSource(Projectile), pos, Vector2.Zero, ModContent.ProjectileType<BetsyDD2Shield>(), 0, 0, ai0: Projectile.whoAmI);
                }
                timer++;
                if (timer == timeToDet)
                {
                    foreach (var proj in Main.ActiveProjectiles)
                    {
                        if (proj.type == ModContent.ProjectileType<BetsyDarkMageClone>())
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
                            new ElectricSpark(Projectile.Center, scale * 25 * Vector2.UnitX.RotatedBy(rot), Color.RoyalBlue, scale, 90).Spawn();
                        }


                        // toward betsy
                        for (int i = 0; i < 2; i++)
                        {
                            float rot = Projectile.AngleTo(betsy.Center) - (safeRot / 2) + Main.rand.NextFloat(0f, safeRot);
                            float scale = Main._rand.NextFloat(0f, 2f);
                            new ElectricSpark(Projectile.Center, scale * 6 * Vector2.UnitX.RotatedBy(rot), Color.RoyalBlue, scale, 90).Spawn();
                        }
                    }
                    
                    if (timer % 2 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f}, Projectile.Center);
                    }

                }

                if (timer > timeToDet + 180)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                timer--;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.Electrified, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            NPC betsy = Main.npc[(int)Projectile.ai[0]];
            if (!betsy.Alive() || betsy.type != NPCID.DD2Betsy)
            {
                Projectile.Kill();
                return false;
            }

            int timeToDet = WorldSavingSystem.MasochistModeReal ? 120 : 180;
            float ai2;
            float retractTime = 20;
            if (Projectile.ai[1] == 0)
            {
                ai2 = MathHelper.Lerp(0, 58, (-1)*(180f+Projectile.ai[2]) / 360f);
            }
            else
            {
                if (Projectile.ai[2] > timeToDet)
                {
                    ai2 = 2f * (Projectile.ai[2] - timeToDet);
                }
                else if (Projectile.ai[2] > timeToDet - retractTime)
                {
                    ai2 = MathHelper.Lerp(0, 58, (retractTime - (Projectile.ai[2] - timeToDet + retractTime)) / retractTime);
                }
                else
                {
                    ai2 = 58;
                }
            }
            ai2 = MathHelper.Clamp(ai2, 0, 58);

            Color color = Color.RoyalBlue;
            Vector2 pos = Projectile.Center;

            // aimed to betsy

            float radius = 20 * MathHelper.Min(15, ai2);
            float arcWidth = Projectile.ai[2] <= timeToDet ? 0 : 1.04f * MathHelper.PiOver4;
            float arcAngle = Projectile.AngleTo(betsy.Center);

            var blackTile = TextureAssets.MagicPixel;
            var noise = FargoAssets.PerlinNoise;
            if (!blackTile.IsLoaded || !noise.IsLoaded)
            {
                return false;
            }
          

            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.DestroyerScanTelegraph");
            shader.TrySetParameter("colorMult", 7.35f);
            shader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            shader.TrySetParameter("radius", radius);
            shader.TrySetParameter("arcAngle", arcAngle.ToRotationVector2());
            shader.TrySetParameter("arcWidth", arcWidth);
            shader.TrySetParameter("anchorPoint", pos);
            shader.TrySetParameter("screenPosition", Main.screenPosition);
            shader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            shader.TrySetParameter("maxOpacity", Projectile.ai[2] > timeToDet ? 0.35f : 0.2f);
            shader.TrySetParameter("color", color.ToVector4());
            
            // rest

            float radius2 = Projectile.ai[1] == 0 ? 20 * 58 : 20 * ai2;
            float lerp = LumUtils.Saturate(Projectile.ai[2] / 90f);
            float arcWidth2 = Projectile.ai[1] == 0 ? 1.3f * MathHelper.Pi : MathHelper.Lerp(1.3f * MathHelper.Pi, (MathHelper.Pi - 0.05f * MathHelper.PiOver4), lerp);
            float arcAngle2 = MathHelper.Pi + Projectile.AngleTo(betsy.Center);

            Main.spriteBatch.GraphicsDevice.Textures[1] = noise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, shader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float colorMult = Projectile.ai[1] == 0 ? ai2 / 58f : 1;

            ManagedShader shader2 = ShaderManager.GetShader("FargowiltasSouls.DestroyerScanTelegraph");
            shader2.TrySetParameter("colorMult", 7.35f);
            shader2.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            shader2.TrySetParameter("radius", radius2);
            shader2.TrySetParameter("arcAngle", arcAngle2.ToRotationVector2());
            shader2.TrySetParameter("arcWidth", arcWidth2);
            shader2.TrySetParameter("anchorPoint", pos);
            shader2.TrySetParameter("screenPosition", Main.screenPosition);
            shader2.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            shader2.TrySetParameter("maxOpacity", Projectile.ai[2] > timeToDet ? 0.5f : 0.4f);
            shader2.TrySetParameter("color", color.ToVector4() * colorMult);


            Main.spriteBatch.GraphicsDevice.Textures[1] = noise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, shader2.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt2 = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt2, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            return false;
        }
    }
}
