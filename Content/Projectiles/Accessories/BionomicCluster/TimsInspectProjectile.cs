using Fargowiltas.Common.Configs;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;

namespace FargowiltasSouls.Content.Projectiles.Accessories.BionomicCluster
{
    internal class TimsInspectProjectile : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.FargoSouls().DeletionImmuneRank = 2;
            Projectile.scale = 0.1f;
            Projectile.timeLeft = 5;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Projectile.timeLeft++;
            Player p = Main.player[Projectile.owner];
            bool effect = p.FargoSouls().TimsInspect && p.HasEffect<TimsInspectEffect>();
            if (!p.active || p.dead || Projectile.scale < 0.1f)
                Projectile.Kill();
            Projectile.Center = p.Center;

            float maxScale = 8;
            if (Projectile.ai[0] < 60)
            {
                Projectile.scale = maxScale - (float)Math.Pow(((Projectile.ai[0] - 60) / 21.4f), 2);
            }
            else
            {
                Projectile.ai[0] = 60;
                Projectile.scale = 8;
            }
            if (effect)
                Projectile.ai[0]++;
            else
                Projectile.ai[0]--;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!Projectile.owner.IsWithinBounds(Main.maxPlayers))
            {
                Projectile.Kill();
                return false;
            }
            Player player = Main.player[Projectile.owner];
            if (!player.Alive())
            {
                Projectile.Kill();
                return false;
            }

            Color darkColor = Color.DarkBlue;
            Color mediumColor = Color.MediumBlue;
            Color lightColor2 = Color.Lerp(Color.Blue, Color.White, 0.35f);

            Vector2 auraPos = player.Center;
            float radius = Projectile.width * Projectile.scale;
            var target = Main.LocalPlayer;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = 0.4f * Projectile.Opacity * ModContent.GetInstance<FargoClientConfig>().TransparentFriendlyProjectiles;

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
            borderShader.TrySetParameter("opacityAmp", 3f);

            Main.spriteBatch.GraphicsDevice.Textures[1] = diagonalNoise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, borderShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            if (Main.myPlayer != player.whoAmI)
                return false;

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.Distance(Projectile.Center) > 3000)
                    continue;

                int potion = -1;
                // Check drops for tim's conc
                List<IItemDropRule> rulesforNPC = Main.ItemDropsDB.GetRulesForNPCID(n.type, false);
                List<DropRateInfo> list = new List<DropRateInfo>();
                DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed(1f);
                foreach (IItemDropRule rule in rulesforNPC)
                    rule.ReportDroprates(list, ratesInfo);
                foreach (DropRateInfo rInfo in list)
                {
                    if (rInfo.conditions == null)
                        continue;

                    foreach (IItemDropRuleCondition cond in rInfo.conditions)
                    {
                        if (cond is TimsConcoctionDropCondition) // Item is from tim's
                            potion = rInfo.itemId;
                    }
                }

                if (potion == -1)
                    continue;

                float oScale = MathHelper.Lerp(0, 1f, Projectile.scale / 8f);
                float dist = (n.Center - Projectile.Center).Length();
                Texture2D potText = TextureAssets.Item[potion].Value;
                Rectangle rect = potText.Frame();
                Vector2 pos = dist > radius ? Projectile.Center + (0.97f * radius * Vector2.UnitX.RotatedBy((n.Center - Projectile.Center).ToRotation()) - rect.Size()) + rect.Size() / 2 : n.Center - rect.Size() / 2;

                
                float distOpac = MathHelper.Min(1, 1 - MathHelper.Lerp(0f, 0.9f, (dist - radius)/(radius * 0.9f)));
                float actualOpac = 0.8f * Projectile.Opacity * oScale * distOpac;
                Main.EntitySpriteDraw(potText, pos - Main.screenPosition + potText.Size() / 2, rect, Color.White * actualOpac, 0, rect.Size() / 2, distOpac * oScale, SpriteEffects.None);
            }
            
            return false;
        }
    }
}
