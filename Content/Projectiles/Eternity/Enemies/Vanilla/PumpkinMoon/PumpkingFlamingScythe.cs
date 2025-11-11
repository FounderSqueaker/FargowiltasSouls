using FargowiltasSouls.Content.Bosses.AbomBoss;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.PumpkinMoon
{
    public class PumpkingFlamingScythe : ModProjectile
    {
        private int p = -1;

        public override string Texture => "Terraria/Images/Projectile_329";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Flaming Scythe");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        bool renderTrail;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;

            Projectile.light = 0.25f;
            Projectile.tileCollide = false;
            Projectile.hide = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.abomBoss, ModContent.NPCType<AbomBoss>()))
            {
                CooldownSlot = ImmunityCooldownID.Bosses;
                renderTrail = true;
            }
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.hide = false;
                Projectile.rotation = Main.rand.NextFloat((float)Math.PI / 2);
                Projectile.direction = Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            }

            if (++Projectile.localAI[0] > 30 && Projectile.localAI[0] < 120)
            {
                Projectile.velocity *= Projectile.ai[0];
            }

            if (Projectile.localAI[0] > 60 && Projectile.localAI[0] < 180)
            {
                if (p == -1)
                    p = Player.FindClosest(Projectile.Center, 0, 0);
                if (p != -1)
                {
                    float rotation = Projectile.velocity.ToRotation();
                    Vector2 vel = Main.player[p].Center - Projectile.Center;
                    float targetAngle = vel.ToRotation();
                    Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0f).RotatedBy(rotation.AngleLerp(targetAngle, Projectile.ai[1]));
                }
            }

            Projectile.rotation += Projectile.velocity.Length() * 0.015f * Math.Sign(Projectile.velocity.X);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width,
                    Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 3f);
                Main.dust[dust].velocity *= 1.4f;
            }

            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width,
                    Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 7f;
                dust = Dust.NewDust(Projectile.position, Projectile.width,
                    Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 3f;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.abomBoss, ModContent.NPCType<AbomBoss>()))
            {
                target.AddBuff(BuffID.Bleeding, 240);
                if (WorldSavingSystem.EternityMode)
                    target.AddBuff(ModContent.BuffType<AbomFangBuff>(), 240);
            }
            else
            {
                target.AddBuff(BuffID.OnFire, 900);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            if (renderTrail)
            {
                for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
                {
                    Color color27 = color26;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Vector2 value4 = Projectile.oldPos[i];
                    float num165 = Projectile.oldRot[i];
                    Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}