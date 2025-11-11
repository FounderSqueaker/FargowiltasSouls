using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using FargowiltasSouls.Content.Buffs.Eternity;
using Terraria.GameContent.Events;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class DarkBolt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2DarkMageBolt;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        public ref float target => ref Projectile.ai[0];
        public ref float state => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.05f;
            Player player = Main.player[(int)target];
            Lighting.AddLight(Projectile.Center, TorchID.Pink);
            
            if (state == 0)
            {
                Projectile.velocity *= 1.03f;
                if (Projectile.velocity.Length() > 20)
                {
                    FargoSoulsUtil.DustRing(Projectile.Center, 20, DustID.PinkTorch, 2f);
                    SoundEngine.PlaySound(SoundID.Item72, Projectile.Center);
                    Projectile.velocity = 6 * Vector2.UnitX.RotatedBy((player.Center - Projectile.Center).ToRotation());
                    state = 1;
                }
            }
            if (state == 1)
                Projectile.velocity *= 1.05f;

            if (Projectile.ai[2] < 1)
                Projectile.ai[2] += 1f / 30;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(ModContent.BuffType<HexedBuff>(), 60);
            target.FargoSouls().HexedInflictor = Projectile.GetSourceNPC().whoAmI;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            Rectangle rectangle = new(0, 0, texture2D13.Width, texture2D13.Height);
            Vector2 origin2 = texture2D13.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            Main.spriteBatch.UseBlendState(BlendState.Additive);
            for (int k = Projectile.oldPos.Length - 1; k >= 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + origin2;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / ((float)Projectile.oldPos.Length * 2));
                color *= 1.75f;
                Main.EntitySpriteDraw(texture2D13, drawPos, rectangle, color, Projectile.rotation, origin2, Projectile.scale * Projectile.ai[2], spriteEffects, 0);
            }

            Color glowColor = Color.White * 0.7f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12).ToRotationVector2() * 2f * Projectile.scale * Projectile.ai[2];

                Main.EntitySpriteDraw(texture2D13, drawPosition + afterimageOffset, rectangle, glowColor, Projectile.rotation, origin2, Projectile.scale * Projectile.ai[2], spriteEffects, 0);
            }
            Main.spriteBatch.ResetToDefault();

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition, rectangle, Color.White, Projectile.rotation, origin2, Projectile.scale * Projectile.ai[2], spriteEffects, 0);
            return false;
        }
    }
}
