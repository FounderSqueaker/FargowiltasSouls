using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class FlyingDragonHostile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_684";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 7;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 1.044f;
            Projectile.direction = (int)Projectile.HorizontalDirectionTo(Projectile.Center + Projectile.velocity);

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, Scale: 2f);

            base.AI();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.WitheredArmor, 240);
            target.AddBuff(BuffID.WitheredWeapon, 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = TextureAssets.Projectile[Type].Value;
            Rectangle rectangle = texture2D13.Frame();
            Vector2 origin2 = rectangle.Size() / 2;
            Color glowColor = Color.Red;
            SpriteEffects flip = SpriteEffects.None;

            float x12 = (Projectile.localAI[0] * ((float)Math.PI * 2f) / 30f).ToRotationVector2().X;
            Color color39 = new Color(220, 40, 30, 40);
            color39 *= 0.75f + 0.25f * x12;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float opac = Projectile.Opacity * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length);
                Color color = Color.Lerp(Color.White, Color.Red, 0.3f + 0.7f * ((float)(i) / Projectile.oldPos.Length));
                Main.EntitySpriteDraw(texture2D13, Projectile.oldPos[i] - Main.screenPosition + Projectile.Hitbox.Size() / 2 + new Vector2(0f, Projectile.gfxOffY), rectangle, color * opac, Projectile.rotation, origin2, Projectile.scale, flip);
            }

            for (int num168 = 0; num168 < 8; num168++)
            {
                Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + Projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 4f * (float)num168) * (4f + 1f * x12), rectangle, color39, Projectile.rotation, origin2, Projectile.scale, flip);
            }

            FargoSoulsUtil.GenericProjectileDraw(Projectile, Color.WhiteSmoke * 0.9f);

            return false;
        }
    }
}
