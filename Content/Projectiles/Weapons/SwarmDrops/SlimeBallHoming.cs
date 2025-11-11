using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Accessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class SlimeBallHoming : SlimeBall
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/SwarmWeapons", "SlimeSlingingSlasherSlimeBalls");

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.DamageType = DamageClass.Melee;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.ai[1] = Main.rand.Next(12);
            Projectile.ai[1] = Main.rand.Next(3);
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int dust = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.TintableDust, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 200, Color.Blue, 1f);
                Main.dust[dust].noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            float fade = 14f;
            if (Projectile.timeLeft < fade)
                Projectile.Opacity -= 1f / fade;

            if (++Projectile.frameCounter % 4 == 0)
            {
                if (++Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }
        }

        public override void OnKill(int timeleft)
        {
            for (int i = 0; i < 10; i++)
            {
                int num469 = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.TintableDust, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 200, Color.Blue, 1f);
                Main.dust[num469].noGravity = true;
                Main.dust[num469].velocity *= 2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new((texture2D13.Width / 12) * (int)Projectile.ai[1], y3, texture2D13.Width / 12, num156);
            rectangle.Inflate(0, -2);
            Vector2 origin2 = rectangle.Size() / 2f;
            SpriteEffects effects = SpriteEffects.None;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}
