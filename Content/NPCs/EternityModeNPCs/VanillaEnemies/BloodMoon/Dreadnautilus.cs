using System;
using System.Collections.Generic;
using System.Linq;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon
{
    public class Dreadnautilus : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BloodNautilus);
        public override void SetStaticDefaults()
        {
            NPCID.Sets.CantTakeLunchMoney[NPCID.BloodNautilus] = true; //cause he sucks in enemies and itd be weird idk
        }
        public override void SetDefaults(NPC npc)
        {
            npc.npcSlots = 10;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            if (npc.ai[0] == -2 && npc.ai[2] < 50) return false;
            return base.CanHitPlayer(npc, target, ref CooldownSlot);
        }

        public float rotspeed;
        public bool OutofSpin;
        public int BatCount;
        public float stalltime;
        static Dictionary<int, Vector2> StoredVel = [];
        public int ShotCount;
        public override bool SafePreAI(NPC npc)
        {
            if (!npc.HasValidTarget)
                npc.velocity.Y -= 1f;
            Player target = Main.player[npc.target];
            bool coag = npc.FargoSouls().BloodDrinker;
            Lighting.AddLight(npc.Center, TorchID.Crimson);
            //Main.NewText($"{npc.ai[0]}, {npc.ai[1]}, {npc.ai[2]}, {npc.ai[3]}");

            if (npc.ai[0] == -2 || npc.ai[0] == -3) //vanilla rotation
            {
                float num15 = npc.Center.DirectionTo(target.Center).ToRotation() - 0.47123894f * npc.spriteDirection;
                if (npc.spriteDirection == -1)
                    num15 += (float)Math.PI;
                if (npc.ai[2] < 110)
                {
                    npc.direction = (npc.Center.X < target.Center.X) ? 1 : (-1);
                    if (npc.spriteDirection != npc.direction)
                    {
                        npc.spriteDirection = npc.direction;
                        npc.rotation = 0f - npc.rotation;
                        num15 = 0f - num15;
                    }
                    npc.rotation = npc.rotation.AngleTowards(num15, 0.1f);
                }
            }
            BloodNautilus_GetMouthPositionAndRotation(npc, out var mPos, out var mDir);
            switch ((int)npc.ai[0])
            {
                case -1: //vanilla spawn animation
                    if (npc.ai[2] >= 0)
                    {
                        npc.ai[0] = -2; //straight to suckin
                        npc.ai[2] = 0;
                        npc.velocity.Y = -20;
                    }
                    break;

                case -2: //suck enemies attack
                    npc.TargetClosest();
                    npc.velocity *= 0.95f;
                    if (npc.ai[2]++ > 5f && npc.alpha > 0)
                        npc.alpha -= 10;

                    if (npc.ai[2] == 90 && FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 1350);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 1500);
                        SoundEngine.PlaySound(SoundID.Zombie93 with { Volume = 3f, Pitch = 0.5f }, npc.Center);
                    }
                    if (npc.ai[2] >= 110)
                    {
                        if (npc.ai[2] == 110 && FargoSoulsUtil.HostCheck)
                        {
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DreadSuck>(), 0, 0, -1, npc.whoAmI, 50, 100);
                            NetMessage.SendData(MessageID.SyncProjectile, number: proj);
                        }

                        npc.rotation += rotspeed;
                        if (npc.ai[2] > 250 && rotspeed > 0 && npc.rotation > 0) rotspeed -= 0.001f;
                        else if (rotspeed < 0.05f) rotspeed += 0.001f;

                        if (npc.ai[2] >= 300)
                        {
                            npc.ai[0] = npc.ai[1] = npc.ai[2] = npc.ai[3] = 0;
                            rotspeed = 0f;
                        }
                    }
                    return false;

                case 0: //between attacks
                    if (npc.HasValidTarget && !OutofSpin && npc.Center.Distance(target.Center) > 320)
                    {
                        float modifier = (npc.Distance(Main.player[npc.target].Center) > 900 ? 0.3f : 0.1f) * (coag ? 2 : 1);
                        npc.velocity += modifier * npc.SafeDirectionTo(Main.player[npc.target].Center);
                        npc.position += npc.velocity;
                    }
                    if (OutofSpin) npc.velocity *= 0.9f;
                    if (npc.ai[1] == 20) OutofSpin = false;
                    break;

                case 1: //spinning around player
                    if (npc.ai[1] <= 90)
                    {
                        //stall slightly for more reaction time
                        if (npc.ai[1] == 85 && stalltime < 10)
                        {
                            npc.ai[1] -= 4;
                            stalltime++;
                        }

                        //spawn bats
                        if (npc.ai[1] % 20 == 0 && FargoSoulsUtil.HostCheck)
                        {
                            BatCount++;
                            SoundEngine.PlaySound(SoundID.Item78, mPos);
                            Vector2 vel = 10 * Vector2.UnitX.RotatedBy(mDir - MathHelper.TwoPi);
                            Projectile.NewProjectile(npc.GetSource_FromThis(), mPos, vel, ModContent.ProjectileType<BloodSpinShot>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.whoAmI, BatCount, -1);
                        }

                        BreatheIn(mPos, mDir);
                    }
                    else //returning false here
                    {
                        //vanilla spin, modified to be larger radius
                        npc.ai[1]++;
                        float rotationpointithinkidk = npc.Center.DirectionFrom(target.Center).ToRotation() - 0.47123894f * npc.spriteDirection;
                        if (npc.spriteDirection == -1)
                            rotationpointithinkidk += (float)Math.PI;
                        npc.position += npc.netOffset;
                        npc.rotation = npc.rotation.AngleLerp(rotationpointithinkidk, 0.05f);
                        Vector2 rot = mDir.ToRotationVector2();
                        if (npc.Center.Distance(target.Center) > 150f || npc.velocity.Length() < 1) //fix vanilla bug where spin dies if u stand on him
                            npc.velocity = rot * -28f + npc.Center.DirectionTo(target.Center) * 1.5f;
                        for (int m = 0; m < 4; m++)
                        {
                            Dust d = Dust.NewDustDirect(mPos + rot * 60f - new Vector2(15f), 30, 30, DustID.Blood, 0f, 0f, 0, Color.Transparent, 1.5f);
                            d.velocity = d.position.DirectionFrom(mPos + Main.rand.NextVector2Circular(5f, 5f)) * d.velocity.Length();
                            d.position -= rot * 60f;
                            d = Dust.NewDustDirect(mPos + rot * 100f - new Vector2(20f), 40, 40, DustID.Blood, 0f, 0f, 100, Color.Transparent, 1.5f);
                            d.velocity = d.position.DirectionFrom(mPos + Main.rand.NextVector2Circular(10f, 10f)) * (d.velocity.Length() + 5f);
                            d.position -= rot * 100f;
                        }
                        npc.position -= npc.netOffset;

                        if (npc.ai[1] >= 360) //originally 270
                        {
                            OutofSpin = true;
                            BatCount = 0;
                            stalltime = npc.ai[0] = npc.ai[1] = 0f;
                            npc.netUpdate = true;
                            npc.TargetClosest();
                            npc.ai[2] = npc.direction;
                        }

                        return false;
                    }
                    break;

                case 2: //blood shotgun, replaces vanilla blood spit
                    if (npc.ai[1] < 90 || (npc.ai[1] - 90) % 30 >= 10)
                    {
                        npc.direction = (npc.Center.X < target.Center.X) ? 1 : (-1);
                        if (npc.spriteDirection != npc.direction)
                        {
                            npc.spriteDirection = npc.direction;
                            npc.rotation = 0f - npc.rotation;
                        }
                    }
                    float num12 = npc.Center.DirectionTo(target.Center).ToRotation() - 0.47123894f * (float)npc.spriteDirection;
                    if (npc.spriteDirection == -1)
                        num12 += (float)Math.PI;
                    if (npc.spriteDirection != npc.direction)
                        num12 = 0f - num12;

                    if (npc.ai[1] < 90)
                    {
                        npc.position += npc.netOffset;
                        npc.velocity *= 0.95f;
                        npc.rotation = npc.rotation.AngleLerp(num12, 0.2f);
                        BreatheIn(mPos, mDir);
                        npc.ai[1]++;
                        npc.position -= npc.netOffset;
                    }
                    else if (npc.ai[1] < 180)
                    {
                        npc.position += npc.netOffset;
                        npc.velocity *= 0.95f;
                        if (npc.HasValidTarget && !OutofSpin && npc.Center.Distance(target.Center) > 320 && (npc.ai[1] - 90) % 30 >= 10)
                            npc.velocity += (coag ? 1f : 0.5f) * npc.SafeDirectionTo(Main.player[npc.target].Center);

                        if ((npc.ai[1] - 90) % 30 >= 10) 
                            npc.rotation = npc.rotation.AngleLerp(num12, 0.8f);

                        if ((npc.ai[1] - 90) % 30 == 0)
                        {
                            ShotCount = Main.rand.Next(5, 11);
                            for (int k = 0; k < ShotCount; k++)
                            {
                                StoredVel[k] = mDir.ToRotationVector2() * 10f + Main.rand.NextVector2Square(-6f, 6f);
                                int p = Projectile.NewProjectile(npc.GetSource_FromThis(), mPos - mDir.ToRotationVector2() * 5f, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), 0, 0, Main.myPlayer, 8, npc.whoAmI, StoredVel[k].ToRotation());
                            }
                        }
                        if ((npc.ai[1] - 90) % 30 == 10)
                        {
                            npc.velocity += mDir.ToRotationVector2() * -8f;
                            if (FargoSoulsUtil.HostCheck)
                            {
                                if (!Main.dedServ) SoundEngine.PlaySound(SoundID.Item36 with {Volume = 3f}, mPos);
                                for (int k = 0; k < ShotCount; k++)
                                {
                                    int p = Projectile.NewProjectile(npc.GetSource_FromThis(), mPos - mDir.ToRotationVector2() * 5f, StoredVel[k], ProjectileID.BloodNautilusShot, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer);
                                    if (p.IsWithinBounds(Main.maxProjectiles))
                                    {
                                        Main.projectile[p].localAI[0] = 1; //no spawn sound
                                        Main.projectile[p].ai[0] = -9000; //no gravity
                                        Main.projectile[p].extraUpdates = 1; //speed up
                                        for (int i = 0; i < 8; i++) //projectile dust bc removing sound kills these too
                                        {
                                            Dust obj13 = Main.dust[Dust.NewDust(Main.projectile[p].position, Main.projectile[p].width, Main.projectile[p].height, DustID.Blood, Alpha: 100, Scale: 0.9f)];
                                            obj13.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f + Main.projectile[p].velocity.SafeNormalize(Vector2.Zero) * 2f;
                                            obj13.fadeIn = 1.1f;
                                            obj13.position = Main.projectile[p].Center;
                                        }
                                    }
                                }
                            }
                            for (int j = 0; j < 20; j++)
                            {
                                Dust dust5 = Dust.NewDustDirect(mPos + mDir.ToRotationVector2() * 60f - new Vector2(15f), 30, 30, DustID.Blood, 0f, 0f, 0, Color.Transparent, 1.5f);
                                dust5.velocity = dust5.position.DirectionFrom(mPos + Main.rand.NextVector2Circular(5f, 5f)) * dust5.velocity.Length();
                                dust5.position -= mDir.ToRotationVector2() * 60f;
                                dust5 = Dust.NewDustDirect(mPos + mDir.ToRotationVector2() * 100f - new Vector2(20f), 40, 40, DustID.Blood, 0f, 0f, 100, Color.Transparent, 1.5f);
                                dust5.velocity = dust5.position.DirectionFrom(mPos + Main.rand.NextVector2Circular(10f, 10f)) * (dust5.velocity.Length() + 5f);
                                dust5.position -= mDir.ToRotationVector2() * 100f;
                            }
                        }
                        npc.position -= npc.netOffset;
                    }

                    npc.ai[1] += (npc.ai[1] < 90) ? 1f : 0.5f;
                    if (npc.ai[1] >= 180)
                    {
                        npc.ai[0] = 0;
                        npc.ai[1] = 0;
                        npc.ai[2] = npc.direction;
                        npc.netUpdate = true;
                        npc.TargetClosest();
                    }

                    return false;

                case 3: //glowing, spawning blood squids, ends at ai1=180
                    {
                        if (npc.ai[1] >= 179)
                        {
                            npc.ai[0] = -2;
                            npc.ai[1] = npc.ai[3] = 0;
                            npc.ai[2] = 80;
                        }
                    }
                    break;

                default:
                    break;
            }

            BloodNautilus_GetMouthPositionAndRotation(npc, out var mouthpos, out var mouthdir);

            //suck in and kill enemies that get near mouth regardless of vaccuum state
            foreach (NPC n in Main.npc.Where(n => n.Alive() && n.Hostile() && !n.boss && n.type != npc.type && (n.lifeMax <= npc.lifeMax || n.type == NPCID.BloodEelHead)))
            {
                if (mouthpos.Distance(n.Center) < npc.width / 2)
                {
                    npc.AddBuff(ModContent.BuffType<BloodDrinkerBuff>(), 360);
                    CombatText.NewText(n.Hitbox, Color.Red, n.life);

                    n.velocity = Vector2.Zero; //dont sent gores flying
                    n.life = 0;
                    n.HitEffect();
                    n.checkDead();
                    n.active = false;
                }
            }

            return base.SafePreAI(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
        }

        public override bool PreKill(NPC npc)
        {
            return base.PreKill(npc);
        }

        public void BreatheIn(Vector2 mPos, float mDir)
        {
            float randRot = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
            Color color = Color.Lerp(Color.GhostWhite, Color.DarkRed, Main.rand.NextFloat(0.6f, 0.8f));
            Vector2 rot = Vector2.UnitX.RotatedBy(mDir + randRot - MathHelper.TwoPi);
            new ExpandingBloomParticle(mPos + Main.rand.NextFloat(60, 100) * rot, -3 * rot, color, Vector2.One, Vector2.One * 0.1f, 30).Spawn();
        }
        public static void BloodNautilus_GetMouthPositionAndRotation(NPC npc, out Vector2 mouthPosition, out float mouthDirection)
        {
            float num = npc.rotation + 0.47123894f * (float)npc.spriteDirection;
            if (npc.spriteDirection == -1)
                num += (float)Math.PI;

            mouthDirection = num;
            mouthPosition = npc.Center + mouthDirection.ToRotationVector2() * 50f;
        }
    }

    public class BloodSquid : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BloodSquid);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            //npc.knockBackResist += 0.1f;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            //FargoSoulsUtil.PrintAI(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 1200);
        }
    }
}
