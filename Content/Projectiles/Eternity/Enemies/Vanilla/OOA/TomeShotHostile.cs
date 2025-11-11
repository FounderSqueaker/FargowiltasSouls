using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class TomeShotHostile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_712";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.2f;
            Projectile.extraUpdates = 1;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 300;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            Projectile.velocity *= 1.015f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Animate(5);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;


            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color glowColor = Color.White * 0.7f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.spriteBatch.UseBlendState(BlendState.Additive);
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12).ToRotationVector2() * 4f * Projectile.scale;

                Main.EntitySpriteDraw(texture2D13, drawPosition + afterimageOffset, rectangle, glowColor, Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            }
            Main.spriteBatch.ResetToDefault();

            Color drawColor = Projectile.GetAlpha(lightColor);
            if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                drawColor = Projectile.GetAlpha(Color.White);
            Main.EntitySpriteDraw(texture2D13, drawPosition, rectangle, drawColor, Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            
            return false;
        }
    }
}
