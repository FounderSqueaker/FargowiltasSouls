using FargowiltasSouls.Assets.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.TrojanSquirrel
{
    public class TrojanSnowball : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.timeLeft = 600;

            Projectile.coldDamage = true;
        }

        public override void AI()
        {   
            if (Projectile.timeLeft == ContentSamples.ProjectilesByType[Type].timeLeft)
                Projectile.frame = Main.rand.Next(2);
            
            Projectile.velocity.Y += Projectile.ai[0];
            Projectile.rotation += 0.22f;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TrojanSnowPile>(), 0, 0, Main.myPlayer);
            SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/SnowballSplat") with { Variants = [1, 2, 3] }, Projectile.Center);
            for (int index1 = 0; index1 < 5; ++index1)
            {
                int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SnowBlock);
                Dust dust = Main.dust[index2];
                dust.velocity *= 0.6f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects effects = SpriteEffects.None;

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class TrojanSnowPile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == ContentSamples.ProjectilesByType[Type].timeLeft)
                Projectile.frame = Main.rand.Next(2);
            
            if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                Projectile.velocity.Y += 0.32f;
            }
            else
            {
                Projectile.velocity.Y -= 0.12f;
            }

            if (++Projectile.ai[0] >= 50)
            {
                Projectile.Opacity = MathHelper.Lerp(1, 0, ++Projectile.ai[1] * 0.03f);
                if (Projectile.Opacity <= 0)
                {
                    Projectile.Kill();
                }
            }

        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            float y = 1;
            float x = 1;
            SpriteEffects effects = SpriteEffects.None;
            if (Projectile.ai[0] >= 50)
            {
                y = MathHelper.Lerp(1, 0, Projectile.ai[1] * 0.03f);
                x = MathHelper.Lerp(1, 1.2f, Projectile.ai[1] * 0.03f);
                if (x >= 1.2f)
                {
                    x = 1.2f;
                }
            }
            Vector2 scale = new(x, y);
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0, 5), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2 + new Vector2(0, 5), scale, effects, 0);
            return false;
        }
    }
}