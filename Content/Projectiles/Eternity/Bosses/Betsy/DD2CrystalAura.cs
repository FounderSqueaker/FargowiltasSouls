using Fargowiltas.Common.Configs;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using static FargowiltasSouls.FargoSoulsSets;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class DD2CrystalAura : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");

        public override void SetDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 2;
            Projectile.FargoSouls().DeletionImmuneRank = 2;
        }

        public override void AI()
        {
            int maxRadius = 2000;
            int auraFalloff = 4000;

            Projectile.timeLeft++;

            NPC crystal = EModeDD2Event.GetEterniaCrystal();
            if (crystal == null || DD2Event.LostThisRun || DD2Event.WonThisRun)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = crystal.Center;
            Projectile.velocity = Vector2.Zero;

            Projectile.ai[1]++;
            if (Projectile.ai[1] <= 90)
            {
                Projectile.width += (int)(maxRadius / 90);
                return;
            }

            if (Projectile.ai[2] > 0)
            {
                Projectile.ai[2]--;
            }
            foreach (var p in Main.ActivePlayers)
            {
                float dist = p.Distance(Projectile.Center);
                if (dist > maxRadius && dist < auraFalloff)
                {
                    float count = dist / 10;
                    for (int i = 0; i < count; i++)
                    {
                        Dust d = Dust.NewDustPerfect(Vector2.Lerp(crystal.Center - crystal.height / 2 * Vector2.UnitY, p.Center, i / count), DustID.PurpleTorch, Vector2.Zero, 0);
                        d.noLight = true;
                        d.noGravity = true;
                    }

                    if (Projectile.ai[1] % 10 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.1f }, p.Center);
                    }

                    Lighting.AddLight(p.Center, TorchID.Purple);
                    FargoSoulsUtil.AddDebuffFixedDuration(p, ModContent.BuffType<ShadowflameBuff>(), 2);

                    if (Projectile.ai[2] <= 0) // damage the crystal
                    {
                        FargoSoulsUtil.ScreenshakeRumble(1.5f);
                        SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact with { Volume = 2f }, p.Center);
                        crystal.SimpleStrikeNPC(WorldSavingSystem.MasochistModeReal ? 1000 : 400, (int)p.HorizontalDirectionTo(crystal.Center));
                        Projectile.ai[2] = 90;
                    }
                }
            }

            base.AI();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            NPC crystal = EModeDD2Event.GetEterniaCrystal();
            if (crystal == null)
            {
                Projectile.Kill();
                return false;
            }

            Color darkColor = Color.Purple;
            Color mediumColor = Color.Pink;
            Color lightColor2 = Color.White;

            Vector2 auraPos = Projectile.Center;
            float radius = Projectile.width;
            var target = Main.LocalPlayer;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.HarshNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = Projectile.Opacity;

            ManagedShader borderShader = ShaderManager.GetShader("FargowiltasSouls.GenericInnerAura");
            borderShader.TrySetParameter("colorMult", 7.35f);
            borderShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            borderShader.TrySetParameter("radius", radius);
            borderShader.TrySetParameter("anchorPoint", auraPos);
            borderShader.TrySetParameter("screenPosition", Main.screenPosition);
            borderShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            borderShader.TrySetParameter("playerPosition", target.Center);
            borderShader.TrySetParameter("maxOpacity", maxOpacity);
            borderShader.TrySetParameter("darkColor", darkColor.ToVector4());
            borderShader.TrySetParameter("midColor", mediumColor.ToVector4());
            borderShader.TrySetParameter("lightColor", lightColor2.ToVector4());
            borderShader.TrySetParameter("opacityAmp", 3.5f);

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
