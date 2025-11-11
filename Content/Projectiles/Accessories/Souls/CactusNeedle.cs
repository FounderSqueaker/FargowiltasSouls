using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class CactusNeedle : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RollingCactusSpike;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = 336;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = true;
            Projectile.width = Projectile.height = 5;
            Projectile.penetrate = 2; //dies on hit
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 1) // from cactus staff
            {
                Projectile.ai[1] = 0;
                for (int i = 0; i < 5; i++)
                {
                    Dust c = Dust.NewDustDirect(Projectile.Center - Projectile.velocity, Projectile.width, Projectile.height, DustID.JunglePlants, Scale: 1.1f);
                    c.noGravity = true;
                    c.velocity *= 3f;
                }
            }

            if (Projectile.ai[2] == 1) //sticking
            {
                int npc = (int)Projectile.ai[1];
                //Projectile.damage = 0;
                Projectile.aiStyle = -1;
                Projectile.extraUpdates = 1;
                Projectile.ignoreWater = true;
                Projectile.tileCollide = false;

                if (Main.npc[npc].Alive())
                {
                    Projectile.timeLeft++;
                    Projectile.Center = Main.npc[npc].Center - Projectile.velocity * 2f;
                    Projectile.gfxOffY = Main.npc[npc].gfxOffY;
                }
                else Projectile.timeLeft = 0;
            }
            else Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 1)
            {
                if (!target.FargoSouls().Needled)
                {
                    target.FargoSouls().Needled = true;
                    Projectile.ai[2] = 1;
                    Projectile.ai[1] = target.whoAmI;
                    Projectile.aiStyle = -1;
                    Projectile.extraUpdates = 0;
                    Projectile.timeLeft = 300;
                    Projectile.velocity = (Main.npc[target.whoAmI].Center - Projectile.Center) * 1f;
                    Projectile.netUpdate = true;
                }
                else Projectile.timeLeft = 0;
            }
            else
            {
                if (Main.rand.NextBool(3))
                    target.AddBuff(BuffID.Poisoned, Main.rand.Next(60, 120));
            }
        }

        public override bool? CanHitNPC(NPC target) => Projectile.ai[2] == 1 ? false : base.CanHitNPC(target);

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawpos = Projectile.Center;
            if (Projectile.ai[2] != 1)
            {
                Asset<Texture2D> t = TextureAssets.Projectile[Type];
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    Color g = Color.LawnGreen * ((lightColor.R + lightColor.G + lightColor.B) / 3f / 200f);
                    Main.EntitySpriteDraw(t.Value, Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition, null, g with { A = 150 } * MathHelper.Lerp(1f, 0.5f, (float)i / Projectile.oldPos.Length), Projectile.oldRot[i], t.Size() / 2, MathHelper.Lerp(1f, 0.5f, (float)i / Projectile.oldPos.Length), SpriteEffects.None);
                }
            }
            else drawpos = Projectile.Center + Main.rand.NextVector2CircularEdge(1, 1); // shaky unstable movement
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor, drawPos: drawpos);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust c = Main.dust[Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.JunglePlants, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, Scale: Main.rand.NextBool(2) ? 0.8f : 1f)];
                c.noGravity = true;
            }
        }
    }
}
