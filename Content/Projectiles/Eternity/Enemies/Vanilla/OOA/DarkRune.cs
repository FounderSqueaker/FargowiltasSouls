using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class DarkRune : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2DarkMageRaise;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 82;
            Projectile.height = 82;
            Projectile.timeLeft = 500;
            Projectile.scale = 0.5f;
            Projectile.hostile = true;
            Projectile.tileCollide = false;

            Projectile.Opacity = 0f;
        }

        public ref float target => ref Projectile.ai[0];
        public ref float shootOrder => ref Projectile.ai[2];

        public override void AI()
        {
            Projectile.Opacity += 1f / 45;
            if (!NPC.AnyNPCs(NPCID.DD2DarkMageT1) && !NPC.AnyNPCs(NPCID.DD2DarkMageT3))
            {
                Projectile.Kill();
                return;
            }

            if (500 - Projectile.timeLeft == 1)
            {
                FargoSoulsUtil.DustRing(Projectile.Center, 30, DustID.PinkTorch, 3f, scale: 2f);
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0f, 0.8f));
            Dust.NewDust(Projectile.position, 20, 20, DustID.Shadowflame);
            int timeToShoot = (60 * (int)shootOrder);
            if (timeToShoot > 500 - Projectile.timeLeft)
            {
                Player player = Main.player[(int)target];
                if (!player.active || player.dead)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.ai[1] += 0.08f;
                Projectile.Center = player.Center + 250 * Vector2.UnitY.RotatedBy(Projectile.ai[1]);
            }
            else if (timeToShoot == 500 - Projectile.timeLeft)
            {
                Player player = Main.player[(int)target];
                target = (player.Center - Projectile.Center).ToRotation();
                Projectile.ai[1] = 0;
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
                FargoSoulsUtil.DustRing(Projectile.Center, 30, DustID.PinkTorch, 3f, scale: 2f);
            }
            else
            {
                Projectile.ai[1]++;
                float speed = (Projectile.ai[1] * Projectile.ai[1])/120 - 4f;
                Projectile.velocity = speed * Vector2.UnitX.RotatedBy(target);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.ShadowFlame, 30);
            base.OnHitPlayer(target, info);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Rectangle frame = new(0, 0, texture.Width, texture.Height);
            Vector2 origin2 = frame.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int k = Projectile.oldPos.Length - 1; k >= 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + origin2/2 + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(Color.White) * ((Projectile.oldPos.Length - k) / ((float)Projectile.oldPos.Length * 1.5f));
                Main.EntitySpriteDraw(texture, drawPos, frame, color, Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, origin2, Projectile.scale, spriteEffects);
            return false;
        }
    }
}
