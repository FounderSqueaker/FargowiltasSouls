using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.AbomBoss
{
    public class AbomRitualFTW : BaseArena
    {
        public override string Texture => "FargowiltasSouls/Content/Bosses/AbomBoss/AbomDeathScythe";

        private const float realRotation = MathHelper.Pi / 150f;

        public AbomRitualFTW() : base(realRotation, 900f, ModContent.NPCType<AbomBoss>(), 87) { }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            // DisplayName.SetDefault("Abominationn Seal");
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            CooldownSlot = ImmunityCooldownID.TileContactDamage;
        }
        protected override void Movement(NPC npc)
        {
            Projectile.velocity = npc.Center - Projectile.Center;
            if (npc.ai[0] != 8) //snaps directly to abom when preparing for p2 attack
                Projectile.velocity /= 40f;

            rotationPerTick = realRotation;
        }

        public override void AI()
        {
            base.AI();
            Projectile.rotation += 1f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 0, 0) * Projectile.Opacity * (targetPlayer == Main.myPlayer ? 1f : 0.15f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 240);
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<AbomFangBuff>(), 240);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color outerColor = Color.DarkOrange;
            outerColor.A = 0;

            Color darkColor = outerColor;
            Color mediumColor = Color.Lerp(outerColor, Color.White, 0.75f);
            Color lightColor2 = Color.Lerp(outerColor, Color.White, 0.5f);

            Vector2 auraPos = Projectile.Center;
            float leeway = Projectile.width / 2 * Projectile.scale;
            leeway *= 0.75f;
            float radius = threshold - leeway;
            var target = Main.LocalPlayer;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = Projectile.Opacity * (targetPlayer == Main.myPlayer ? 1f : 0.15f);

            ManagedShader borderShader = ShaderManager.GetShader("FargowiltasSouls.MutantP1Aura");
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