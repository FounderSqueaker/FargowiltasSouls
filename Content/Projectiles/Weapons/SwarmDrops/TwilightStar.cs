using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Items.Weapons.SwarmDrops;
using FargowiltasSouls.Content.Projectiles.Weapons.BossWeapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class TwilightStar : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/SwarmWeapons", Name);

        #region References
        /*
         * 0 = initial deployment
         * 1 = wind up
         * 2 = firing
         * 3 = despawning
         */
        public ref float state => ref Projectile.ai[1];
        public ref float ai0 => ref Projectile.ai[0];
        public ref float timer => ref Projectile.ai[2];
        #endregion

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 38;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.Opacity = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        int drawTimer = 0;
        Color tColor = Color.Lerp(Color.SkyBlue, Color.Blue, 0.6f);

        public override void AI()
        {
            Frame();

            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.ghost)
                Projectile.Kill();

            timer++;
            float velLen = Projectile.velocity.Length() / 5f;

            switch (state)
            {
                case 0: // spawning
                    ai0 = -1;
                    if (!player.channel)
                        Projectile.Kill();
                    if (timer > 20)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            new SmallSparkle(Projectile.Center, Main.rand.NextFloat(2, 5) * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), tColor, Main.rand.NextFloat(0.2f, 0.4f), 14).Spawn();
                        }
                        SoundEngine.PlaySound(SoundID.NPCDeath7 with { Pitch = -0.7f }, Projectile.Center);
                        state = 1;
                    }
                    break;
                case 1: // following mouse
                    if (!player.channel && player.HeldItem.type == ModContent.ItemType<TwilightTome>() && !player.controlUseItem)
                    {
                        SoundEngine.PlaySound(SoundID.Item43 with { Pitch = -0.3f }, Projectile.Center);
                        timer = 0;
                        state = 2;
                        return;
                    }
                    else
                    {
                        Movement(Main.MouseWorld);
                        Projectile.netUpdate = true;
                    }
                    break;
                case 2: // prep to fire
                    if (timer > 3) // let unformed stars despawn
                    {
                        // scale projectile count with star number
                        ai0 = 2 * Main.projectile.Where(p => p.active && p.type == Type && p.ai[1] < 4).Count();

                        timer = 0;
                        state = 3;
                    }
                    break;
                case 3: // firing
                    Projectile.velocity *= 0.95f;

                    if (timer % 15 == 14)
                    {
                        if (ai0 > 0)
                        {
                            float rot = (Main.MouseWorld - Projectile.Center).ToRotation();
                            Vector2 vel = new Vector2(25f, 0f).RotatedBy(rot);
                            float sparkRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                            for (int i = 0; i < 20; i++)
                            {
                                new SmallSparkle(Projectile.Center, Main.rand.NextFloat(1, 2) * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), tColor, Main.rand.NextFloat(0.2f, 0.6f), 18).Spawn();
                            }

                            SoundEngine.PlaySound(SoundID.Item102 with { Pitch = -0.5f, Volume = 0.5f }, Projectile.Center);

                            if (Main.myPlayer == Projectile.owner)
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel,
                                    ModContent.ProjectileType<TwilightStarSpawn>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
                            Projectile.netUpdate = true;
                        }
                        else if (ai0 < 0)
                        {
                            ai0 = -1;
                            timer = 0;
                            state = 4;
                        }

                        ai0--;
                        Projectile.netUpdate = true;
                    }
                    break;
                case 4: // homing charge
                    if (ai0 < 0) // no target
                    {
                        ai0 = FargoSoulsUtil.FindClosestHostileNPC(Projectile.Center, 2000f);

                        if (ai0 >= 0)
                        {
                            FargoSoulsUtil.DustRing(Projectile.Center, 20, DustID.HallowSpray, 3f);
                            SoundEngine.PlaySound(SoundID.DD2_BookStaffCast with { Pitch = -0.5f, Volume = 3f, Variants = [0] }, Projectile.Center);
                        }

                        if (timer > 30)
                        {
                            SoundEngine.PlaySound(SoundID.NPCDeath52 with { Volume = 0.2f, Pitch = -0.2f }, Projectile.Center);
                            Projectile.Kill();
                        }
                    }
                    else
                    {
                        Projectile.extraUpdates = 1;
                        NPC target = Main.npc[(int)ai0];
                        if (target.active)
                            Movement(target.Center);
                        else
                        {
                            Projectile.extraUpdates = 0;
                            ai0 = -1;
                            timer = 0;
                        }
                    }
                    break;
            }

            // visual fx
            if (Projectile.velocity.Length() > 1)
            {
                new AlphaBloomParticle(Projectile.Center, Projectile.velocity * 0.5f, tColor, Vector2.Zero, 1.2f * Projectile.scale * (velLen) * Vector2.One, 25).Spawn();
                float spread = 0.5f;
                if (timer % 10 == 0)
                {
                    float randRot = Projectile.velocity.ToRotation() + Main.rand.NextFloat(-spread, spread);
                    new SmallSparkle(Projectile.Center, (velLen / 5f) * Vector2.UnitX.RotatedBy(randRot), tColor, (velLen) * 0.35f, 24).Spawn();
                }
            }

            if (timer % 22 == 0 && state < 3)
                SoundEngine.PlaySound(SoundID.Item9 with { Pitch = -1f, Volume = 0.1f, MaxInstances = 1 }, Projectile.Center);
            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.8f, 2f));
        }

        public void Frame()
        {
            Projectile.rotation = -Projectile.spriteDirection * Projectile.velocity.Length() / 12f;
            drawTimer++;
            if (drawTimer % 6 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 8)
                    Projectile.frame = 0;
            }
        }

        public void Movement(Vector2 targetPos)
        {
            bool enemy = state == 4;
            float velLen = Projectile.velocity.Length();

            Vector2 posToIdle = targetPos - Projectile.Center;
            float dist = posToIdle.Length();
            Projectile.velocity += 0.25f * Vector2.UnitX.RotatedBy(posToIdle.ToRotation());

            if (Math.Abs((posToIdle.ToRotation() - Projectile.velocity.ToRotation()) / MathHelper.TwoPi) > 0.5f)
            {
                Projectile.velocity *= enemy ? 0.90f : 0.98f;
            }

            float speedCap = enemy ? 8f : 12f;
            if (velLen > speedCap)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= speedCap;
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
            Projectile.netUpdate = true;
        }

        public override bool? CanHitNPC(NPC target) => state >= 4;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            FargoSoulsUtil.DustRing(target.Center, 20, DustID.HallowSpray, 3f);

            base.OnHitNPC(target, hit, damageDone);
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            if (ai0 >= 0)
            {
                SoundEngine.PlaySound(SoundID.Item102 with { Pitch = -0.5f, Volume = 0.5f }, Projectile.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 offset = new(Main.rand.NextFloat(-300, 300), -500);
                        float rot = (Main.npc[(int)ai0].Center - (Projectile.Center + offset)).ToRotation() + Main.rand.NextFloat(-0.03f, 0.03f);
                        Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center + offset, 30 * Vector2.UnitX.RotatedBy(rot),
                            ModContent.ProjectileType<TwilightStarSpawn>(), Projectile.damage / 3, 1f, Projectile.owner);
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.NPCDeath7 with { Pitch = -0.2f }, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                new SmallSparkle(Projectile.Center, Main.rand.NextFloat(2, 5) * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), tColor, Main.rand.NextFloat(0.2f, 0.6f), 24).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float opac = Projectile.Opacity;
            if (state == 0)
                opac = Projectile.Opacity * timer / 20;

            Texture2D text = TextureAssets.Projectile[Type].Value;
            int height = text.Height / Main.projFrames[Type];
            Rectangle frame = new (0, Projectile.frame * height, text.Width, height);

            //float count = 10;
            //for (int i = 0; i < count; i++)
            //{
            //    float scale = (1 - (i / count));
            //    Color c = Color.SkyBlue * opac * (i/count);
            //    Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, frame, lightColor, 0, frame.Size() / 2, Projectile.scale * scale, SpriteEffects.None);
            //}
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor * opac);
            return false;
        }
    }
}
