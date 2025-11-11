using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Common.Graphics.Particles;
using Luminance.Core.Graphics;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
using Terraria.DataStructures;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using FargowiltasSouls.Assets.Sounds;
using System.Threading;
using FargowiltasSouls.Assets.Textures;

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class ReleasedLeashofCthulhu : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/SwarmWeapons", Name);
        private float speed;
        private int DashTimer = 0;
        private int DashAmount = 0;      
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(FargosSoundRegistry.LeashBreak, Main.player[Projectile.owner].Center);
            if (!Main.dedServ)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Bottom, Projectile.velocity * 0.4f, ModContent.Find<ModGore>(Mod.Name, "CollarGore1").Type, Projectile.scale * 0.8f);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Bottom, Projectile.velocity * 0.4f, ModContent.Find<ModGore>(Mod.Name, "CollarGore2").Type, Projectile.scale * 0.8f);
                Gore.goreTime = 5;
            }
            base.OnSpawn(source);
        }
        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 74;
            //Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            //AIType = ProjectileID.Bullet;
            Projectile.scale = 1.5f;
            Projectile.timeLeft = 120;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (speed == 0) //store homing speed
                speed = Projectile.velocity.Length() * 2f;

            float desiredFlySpeedInPixelsPerFrame = speed;
            const float amountOfFramesToLerpBy = 20; // minimum of 1, please keep in full numbers even though it's a float!



            NPC n = FargoSoulsUtil.NPCExists(FargoSoulsUtil.FindClosestHostileNPC(Projectile.Center, 2400, false, true));
            if (n.Alive())
            {
                Projectile.timeLeft = 120;
                if (Projectile.Distance(n.Center) >= 2400)
                {
                    if (!Main.dedServ)
                    {
                        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Left, Projectile.velocity * 0.3f, ModContent.Find<ModGore>(Mod.Name, "ReleasedMechFlailGore1").Type, Projectile.scale);
                        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Right, Projectile.velocity * 0.3f, ModContent.Find<ModGore>(Mod.Name, "ReleasedMechFlailGore2").Type, Projectile.scale);
                    }
                    
                    Projectile.Kill();
                }

                //redirect
                if (++DashTimer <= 45 && DashAmount <= 4)
                {
                    Vector2 desiredVelocity = Projectile.SafeDirectionTo(n.Center + Projectile.DirectionFrom(n.Center) * 200) * desiredFlySpeedInPixelsPerFrame;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 1f / amountOfFramesToLerpBy);
                }
                //if last dash, go back farther
                if (DashTimer <= 45 && DashAmount > 4)
                {
                    Vector2 desiredVelocity = Projectile.SafeDirectionTo(n.Center + Projectile.DirectionFrom(n.Center) * 400) * desiredFlySpeedInPixelsPerFrame;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 1f / amountOfFramesToLerpBy);
                }

                //dash
                if (++DashTimer >= 45 && DashAmount < 4)
                {
                    if (DashAmount < 4)
                    {
                        //SoundEngine.PlaySound(SoundID.ForceRoarPitched with { Volume = 0.05f, MaxInstances = 1 }, Projectile.Center);
                    }
                    Projectile.velocity = Projectile.SafeDirectionTo(n.Center + n.velocity * 10) * 65 + Main.rand.NextVector2Circular(5, 5);
                    DashTimer = 0;
                    DashAmount += 1;
                }
                // bite
                if (DashAmount >= 4 && DashTimer >= 45)
                {
                    Projectile.velocity = Projectile.SafeDirectionTo(n.Center) * 85;
                }
                // kill
                if (DashAmount >= 4 && Projectile.Distance(n.Center) <= 50)
                {
                    if (!Main.dedServ)
                    {
                        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Left, Projectile.velocity * 0.3f, ModContent.Find<ModGore>(Mod.Name, "ReleasedMechFlailGore1").Type, Projectile.scale);
                        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Right, Projectile.velocity * 0.3f, ModContent.Find<ModGore>(Mod.Name, "ReleasedMechFlailGore2").Type, Projectile.scale);
                    }
                    
                    Particle p = new SparkParticle(Projectile.Center, Main.rand.NextVector2Circular(45, 45), Color.DarkRed, 2f, 25);
                    Particle p2 = new SparkParticle(Projectile.Center, Main.rand.NextVector2Circular(35, 35), Color.DarkRed, 2f, 25);
                    Particle p3 = new SparkParticle(Projectile.Center, Main.rand.NextVector2Circular(25, 25), Color.DarkRed, 2f, 25);
                    p.Spawn();
                    p2.Spawn();
                    p3.Spawn();
                    Projectile.Kill();
                }
                Projectile.rotation = Projectile.SafeDirectionTo(n.Center).ToRotation() + (float)Math.PI / 2;



            }
                  
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Particle p = new SparkParticle(target.Center, (Projectile.velocity + Main.rand.NextVector2Circular(45, 45)) * 0.4f, Color.DarkRed, 2f, 25);
            Particle p2 = new SparkParticle(target.Center, (Projectile.velocity + Main.rand.NextVector2Circular(35, 35)) * 0.4f, Color.DarkRed, 1.75f, 25);
            Particle p3 = new SparkParticle(target.Center, (Projectile.velocity + Main.rand.NextVector2Circular(25, 25)) * 0.4f, Color.DarkRed, 1.5f, 25);
            p.Spawn();
            p2.Spawn();
            p3.Spawn();
            base.OnHitNPC(target, hit, damageDone);
        }

        public override void OnKill(int timeLeft)
        {
            for (int num468 = 0; num468 < 20; num468++)
            {
                int num469 = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Silver, -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f, 100, default, 1.5f);
                Main.dust[num469].noGravity = true;
                Main.dust[num469].velocity *= 2f;
                num469 = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Silver, -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f, 100, default, .75f);
                Main.dust[num469].velocity *= 2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                if (i < 7)
                {
                    Color color27 = color26 * 0.4f;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2 - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.None, 0);
                }
            }
            return true;
        }
    }
}