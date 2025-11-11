using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.KingSlime
{
    public class KingSlimeBall : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Bosses/KingSlime", Name);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = Main.rand.Next(18);
        }

        public override void AI()
        {
            Projectile.alpha -= 50;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
            if (Projectile.alpha == 0 && Main.rand.NextBool(3) && (Projectile.tileCollide || !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)))
            {
                Color color = Color.Blue;
                switch (Projectile.ai[0])
                {
                    case <= 2: color = Color.Blue; break;
                    case <= 5: color = Color.LightGreen; break;
                    case <= 8: color = Color.Magenta; break;
                    case <= 11: color = Color.Pink; break;
                    case <= 14: color = Color.LightGray; break;
                    case <= 17: color = Color.Red; break;
                }

                int d = Dust.NewDust(Projectile.position - Projectile.velocity * 3f, Projectile.width, Projectile.height, DustID.TintableDust, 0f, 0f, 150, color, 1.2f);
                Main.dust[d].velocity *= 0.3f;
                Main.dust[d].velocity += Projectile.velocity * 0.3f;
                Main.dust[d].noGravity = true;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = Main.rand.NextBool() ? 1 : 2;
                SoundEngine.PlaySound(SoundID.Item154, Projectile.position);
            }
            Projectile.velocity.Y += 0.3f;

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi / 2f;

            if (++Projectile.frameCounter % 4 == 0)
            {
                if (++Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Slimed, 60);
        }

        public override void OnKill(int timeLeft)
        {
            Color color = Color.Blue;
            switch (Projectile.ai[0])
            {
                case <= 2: color = Color.Blue; break;
                case <= 5: color = Color.LightGreen; break;
                case <= 8: color = Color.Magenta; break;
                case <= 11: color = Color.Pink; break;
                case <= 14: color = Color.LightGray; break;
                case <= 17: color = Color.Red; break;
            }

            for (int i = 0; i < 10; i++)
            {
                int num469 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.TintableDust, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 150, color, 1.2f);
                Main.dust[num469].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new((texture2D13.Width / 18) * (int)Projectile.ai[0], y3, texture2D13.Width / 18, num156);
            rectangle.Inflate(0, -2);
            Vector2 origin2 = rectangle.Size() / 2f;
            SpriteEffects effects = Projectile.localAI[0] == 2 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}