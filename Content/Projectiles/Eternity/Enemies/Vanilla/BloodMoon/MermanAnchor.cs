using System;
using System.Collections.Generic;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class MermanAnchor : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Anchor;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 28;
            Projectile.height = 48;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public Vector2 HoldPos;
        public float RotatePos;
        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            if (owner != null)
            {
                if (owner.Alive()) Projectile.timeLeft = 2;
                Vector2 mermback = owner.Center + new Vector2(-24 * owner.spriteDirection, 0);
                Vector2 slamfront = owner.Center + new Vector2(24 * owner.spriteDirection, 16 + owner.velocity.Y);
                Vector2 swingfront = owner.Center + new Vector2(24 * owner.spriteDirection, owner.velocity.Y);
                HoldPos = owner.ai[0] == 2 ? swingfront : owner.ai[0] == 1 ? slamfront : mermback;
                RotatePos = owner.ai[0] == 2 ? owner.velocity.ToRotation() + MathHelper.PiOver2 : MathHelper.ToRadians(owner.ai[0] == 1 ? 180 : -30) * owner.spriteDirection;

                if (owner.wet && owner.ai[0] == 3)
                {
                    HoldPos = owner.Center + owner.velocity * 10;
                    RotatePos = owner.SafeDirectionTo(Main.player[owner.target].Center).ToRotation() + MathHelper.PiOver2;
                }

                float speed = owner.ai[0] == 3 ? 1f : (owner.ai[0] == 2 || owner.ai[1] > 0) ? 5 : 2;
                Projectile.velocity = FargoSoulsUtil.SmartAccel(Projectile.Center, HoldPos, Projectile.velocity, speed, speed);
                Projectile.rotation = RotateTowards(RotatePos, owner.ai[0] == 2 ? 100 : 10);
                if (owner.ai[0] == 1 && !owner.noTileCollide)
                {
                    Particle p = new SparkParticle(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(0, Projectile.height)), new (0, -2), Color.DarkRed, 0.5f, 25);
                    if (Main.rand.NextBool(3)) p.Spawn();
                }
                if (owner.ai[0] == 2)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        Vector2 vector2_2 = ((float)(Main.rand.NextDouble() * Math.PI) - (float)Math.PI / 2).ToRotationVector2() * Main.rand.Next(3, 8);
                        int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BloodWater, vector2_2.X * 2f, vector2_2.Y * 2f, 100, Scale: 1.4f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity /= 4f;
                        Main.dust[d].velocity -= Projectile.velocity;
                    }
                    if (owner.velocity.Y == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
                    }
                }
            }
            else Projectile.Kill();
        }

        private float RotateTowards(float rotToAlignWith, float speed)
        {
            Vector2 PV = rotToAlignWith.ToRotationVector2();
            Vector2 LV = Projectile.rotation.ToRotationVector2();
            float anglediff = FargoSoulsUtil.RotationDifference(LV, PV);
            return Projectile.rotation.ToRotationVector2().RotatedBy(Math.Sign(anglediff) * Math.Min(Math.Abs(anglediff), speed * MathHelper.Pi / 180)).ToRotation();
        }

        public override bool CanHitPlayer(Player target) => Main.npc[(int)Projectile.ai[0]].ai[0] > 0;
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            float lightLevel = (lightColor.R + lightColor.G + lightColor.B) / 3f / 200f;
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 2;
                Color glowColor = Color.Red * lightLevel;
                Main.EntitySpriteDraw(t.Value, Projectile.Center + afterimageOffset - Main.screenPosition, null, glowColor, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() / 2, 1, SpriteEffects.None);
            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }
    }
}
