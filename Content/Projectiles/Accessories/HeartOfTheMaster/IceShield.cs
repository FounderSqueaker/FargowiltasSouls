using System;
using System.IO;
using System.Linq;
using Fargowiltas.Common.Configs;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class IceShield : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureNPC(NPCID.Flocko);
        private Vector2 mousePos;
        private int syncTimer;

        public Vector2 VisualCenter;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.timeLeft = 60 * 5;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.scale = 2;

            VisualCenter = Projectile.Center;
        }
        public const int ShatterTime = 45;
        public bool Shattered => ShatterTimer >= ShatterTime;
        public ref float ShatterTimer => ref Projectile.ai[0];
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(mousePos.X);
            writer.Write(mousePos.Y);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Vector2 buffer;
            buffer.X = reader.ReadSingle();
            buffer.Y = reader.ReadSingle();
            if (Projectile.owner != Main.myPlayer)
            {
                mousePos = buffer;
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (player.whoAmI == Main.myPlayer && (player.dead || !player.HasEffect<IceShieldEffect>()))
            {
                Projectile.Kill();
                return;
            }
            else
            {
                Projectile.timeLeft = 2;
            }

            if (!Shattered)
                Position(player);
            else
                Projectile.velocity = Vector2.Zero;

            /*
            for (int i = 0; i < 1; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, Scale: 1f);
                Main.dust[d].noGravity = true;
            }
            */

            if (!Shattered)
            {
                Main.projectile.Where(x => x.active && x.hostile && x.damage > 0 && x.Colliding(x.Hitbox, Projectile.Hitbox) && ProjectileLoader.CanDamage(x) != false && ProjectileLoader.CanHitPlayer(x, player) && FargoSoulsUtil.CanDeleteProjectile(x)).ToList().ForEach(x =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int dustId = Dust.NewDust(new Vector2(x.position.X, x.position.Y + 2f), x.width, x.height + 5, DustID.Ice, x.velocity.X * 0.2f, x.velocity.Y * 0.2f, 100);
                        Main.dust[dustId].noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Item50, x.Center);

                    x.Kill();

                    if (ShatterTimer < 1)
                        ShatterTimer = 1;
                });
            }
            if (ShatterTimer < ShatterTime)
                VisualCenter = Projectile.Center;

            if (ShatterTimer > 0)
            {
                ShatterTimer++;
                if (ShatterTimer == ShatterTime)
                {
                    SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
                    Projectile.position = Projectile.Center;
                    Projectile.width *= 3;
                    Projectile.height *= 3;
                    Projectile.Center = Projectile.position + player.DirectionTo(Projectile.position) * 100;
                    Projectile.Opacity = 0;

                    for (int i = 0; i < 40; i++)
                    {
                        int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, Scale: 2f);
                        Main.dust[d].velocity = (Main.dust[d].position - player.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(7, 9);
                        Main.dust[d].noGravity = true;
                    }
                        
                }
                float shatterEnd = 6;
                if (ShatterTimer >= ShatterTime)
                {
                    Projectile.Opacity = 1 - (ShatterTimer - ShatterTime) / shatterEnd;
                }
                if (ShatterTimer > ShatterTime + shatterEnd)
                {
                    Projectile.Kill();
                    return;
                }
            }
        }

        private Vector2 MousePos(Player player) => player.Center + player.Center.SafeDirectionTo(Main.MouseWorld) * 150;

        private void Position(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                mousePos = MousePos(player);

                if (++syncTimer > 20)
                {
                    syncTimer = 0;
                    Projectile.netUpdate = true;
                }
            }

            Vector2 desiredPos = mousePos;
            Projectile.velocity = (desiredPos - Projectile.Center) / 5;
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.SetCooldown<IceShieldEffect>(IceShieldEffect.CD);
            if (player.whoAmI == Main.myPlayer)
                CooldownBarManager.Activate("IceQueenCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "IceQueensShield").Value, Color.LightBlue, () => 1f - Main.LocalPlayer.GetCooldown<IceShieldEffect>() / IceShieldEffect.CD, activeFunction: player.HasEffect<IceShieldEffect>);
        }

        public override bool? CanDamage()
        {
            if (Shattered)
                return base.CanDamage();
            return false;
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
            if (player.whoAmI != Main.myPlayer)
                return false;

            Vector2 auraPos = player.Center;

            Color darkColor = Color.Blue;
            Color mediumColor = Color.LightBlue;

            float radius = player.Distance(VisualCenter);
            float visualWidth = MathF.Sqrt(2 * 50 * 50);
            float circumference = radius * MathF.Tau;
            float fraction = visualWidth / circumference;
            float arcWidth = fraction * MathF.Tau;
            arcWidth *= 1.4f;

            Vector2 nearbyPosition = player.DirectionTo(VisualCenter);
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.CracksNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = Projectile.Opacity * ModContent.GetInstance<FargoClientConfig>().TransparentFriendlyProjectiles;

            ManagedShader innerShader = ShaderManager.GetShader("FargowiltasSouls.IceQueenShield");
            innerShader.TrySetParameter("colorMult", 7.35f);
            innerShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            innerShader.TrySetParameter("radius", radius);
            innerShader.TrySetParameter("anchorPoint", auraPos);
            innerShader.TrySetParameter("screenPosition", Main.screenPosition);
            innerShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            innerShader.TrySetParameter("nearbyPosition", nearbyPosition);
            innerShader.TrySetParameter("maxOpacity", maxOpacity);
            innerShader.TrySetParameter("darkColor", darkColor.ToVector4());
            innerShader.TrySetParameter("midColor", mediumColor.ToVector4());
            innerShader.TrySetParameter("arcWidth", arcWidth);


            Main.spriteBatch.GraphicsDevice.Textures[1] = diagonalNoise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, innerShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, outerShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            //Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            //Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}